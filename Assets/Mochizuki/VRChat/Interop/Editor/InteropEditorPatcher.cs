/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UdonSharpEditor;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

using VRC.Udon;

using Object = UnityEngine.Object;

namespace Mochizuki.VRChat.Interop
{
    [InitializeOnLoad]
    public static class InteropEditorPatcher
    {
        private static readonly MetadataReference[] References;

        static InteropEditorPatcher()
        {
            References = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location), // UnityEngine
                MetadataReference.CreateFromFile(typeof(Editor).Assembly.Location), // UnityEditor
                MetadataReference.CreateFromFile(typeof(UdonBehaviour).Assembly.Location), // VRC.Udon
                MetadataReference.CreateFromFile(typeof(UdonSharpBehaviour).Assembly.Location), // UdonSharp
                MetadataReference.CreateFromFile(typeof(EventListener).Assembly.Location) // Mochizuki.VRChat.Interop
            };

            var harmony = new Harmony("moe.mochizuki.vrchat.interop");

            void ApplyPatchForDrawPublicVariableField()
            {
                // patched to https://github.com/MerlinVR/UdonSharp/blob/v0.19.6/Assets/UdonSharp/Editor/Editors/UdonSharpGUI.cs#L1050
                var original = typeof(UdonSharpGUI).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(w => w.Name == "DrawPublicVariableField");
                var postfix = typeof(InteropEditorPatcher).GetMethod(nameof(DrawPublicVariableFieldPostfix));
                var finalizer = typeof(InteropEditorPatcher).GetMethod(nameof(DrawPublicVariableFieldFinalizer));

                harmony.Patch(original, null, new HarmonyMethod(postfix), null, new HarmonyMethod(finalizer));
            }

            ApplyPatchForDrawPublicVariableField();
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldPostfix(UdonBehaviour currentBehaviour, string symbol, Type variableType, ref object __result)
        {
            if (!ShouldCheckTypeValidation(currentBehaviour, symbol, variableType, __result))
                return;

            var proxyBehaviour = UdonSharpEditorUtility.GetProxyBehaviour(currentBehaviour);
            var variable = proxyBehaviour.GetType().GetField(symbol, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // ReSharper disable once AssignNullToNotNullAttribute
            var attr = variable.GetCustomAttribute<RequestArgumentTypeAttribute>();

            // maybe to heavy

            var callers = new List<(UdonBehaviour, string)>();

            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var components = new List<UdonBehaviour>();

            // currently only supports UdonSharpBehaviour inherited classes
            foreach (var o in rootObjects)
                components.AddRange(o.GetComponentsInChildren<UdonBehaviour>().Where(UdonSharpEditorUtility.IsUdonSharpBehaviour));

            foreach (var behaviour in components)
            {
                var publicVariables = behaviour.publicVariables;
                if (publicVariables == null || publicVariables.VariableSymbols.Count == 0)
                    continue;

                foreach (var s in publicVariables.VariableSymbols)
                    if (publicVariables.TryGetVariableValue(s, out var obj) && obj is Object o && o == (Object) __result && behaviour != currentBehaviour)
                    {
                        callers.Add((behaviour, s));
                        break;
                    }
            }

            foreach (var (behaviour, refSymbol) in callers)
            {
                var cs = ((UdonSharpProgramAsset) behaviour.programSource).sourceCsScript.text;
                var tree = CSharpSyntaxTree.ParseText(cs);

                var compilation = CSharpCompilation.Create("Mochizuki.VRChat.Internal")
                                                   .AddReferences(References)
                                                   .AddSyntaxTrees(tree);
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();

                var argumentPasser = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(w => w.Expression).Where(w =>
                {
                    if (w is MemberAccessExpressionSyntax syntax)
                        return syntax.Name.Identifier.Text == nameof(EventListener.SetArgument);

                    return false;
                }).Cast<MemberAccessExpressionSyntax>();

                var listenerDeclaration = root
                                          .DescendantNodes()
                                          .OfType<FieldDeclarationSyntax>()
                                          .SelectMany(w => w.Declaration.Variables)
                                          .First(w => w.Identifier.Text == refSymbol);

                var declarationSymbol = model.GetDeclaredSymbol(listenerDeclaration);

                foreach (var syntax in argumentPasser)
                {
                    // check the accessor variable is referenced listener?
                    var caller = model.GetSymbolInfo(syntax.Expression);
                    if (!declarationSymbol.Equals(caller.Symbol))
                        continue;

                    // check the accessor method is SetArgument?
                    var invocation = (InvocationExpressionSyntax) syntax.Parent;
                    var method = (MemberAccessExpressionSyntax) invocation.Expression;
                    var identifier = (IdentifierNameSyntax) method.Name;
                    if (identifier.Identifier.Text != nameof(EventListener.SetArgument))
                        continue;

                    // check the argument type equals to requested type?
                    var argument = invocation.ArgumentList.Arguments[0];
                    var declaration = model.GetTypeInfo(argument.Expression);

                    if (!declaration.Type.Equals(model.Compilation.GetTypeByMetadataName(attr.RequestedType.FullName)))
                    {
                        // invalid value
                        EditorGUILayout.HelpBox($"The receiver ({currentBehaviour.name}; this) is requesting {attr.RequestedType.FullName}, but the sender is sending other type(s), so it could not be applied.", MessageType.Warning);
                        return;
                    }
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldFinalizer(Exception __exception)
        {
            if (__exception is ExitGUIException e)
                throw e;
        }

        private static bool ShouldCheckTypeValidation(UdonBehaviour currentBehaviour, string symbol, Type variableType, object @return)
        {
            if (@return == null || variableType != typeof(UdonBehaviour))
                return false;

            if (!UdonSharpEditorUtility.IsUdonSharpBehaviour(currentBehaviour))
                return false;

            if (UdonSharpProgramAsset.GetBehaviourClass((UdonBehaviour) @return) != typeof(EventListener))
                return false;

            var proxyBehaviour = UdonSharpEditorUtility.GetProxyBehaviour(currentBehaviour);
            var variable = proxyBehaviour.GetType().GetField(symbol, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (variable == null)
                return false;

            var attr = variable.GetCustomAttribute<RequestArgumentTypeAttribute>();
            if (attr == null)
                return false;

            return true;
        }
    }
}