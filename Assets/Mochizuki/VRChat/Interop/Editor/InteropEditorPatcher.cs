/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using HarmonyLib;

using JetBrains.Annotations;

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

        [PublicAPI]
        public static void RegisterReferences(Type t)
        {
            References.AddItem(MetadataReference.CreateFromFile(t.Assembly.Location));
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldPostfix(UdonBehaviour currentBehaviour, string symbol, Type variableType, ref object __result)
        {
            var (r, attr) = ShouldCheckTypeValidation(currentBehaviour, symbol, variableType, __result);
            if (!r)
                return;

            var callers = CollectListenerReferences(currentBehaviour, __result);

            foreach (var (behaviour, refSymbol) in callers)
            {
                var (root, model) = CreateAnalysisModels(behaviour);
                var argumentPasser = CollectEventListenerArgumentPassingSyntax(root);
                var declarationSymbol = FindEventListenerVariableDeclarationSyntax(root, model, refSymbol);

                foreach (var syntax in argumentPasser)
                {
                    var (isValidReference, isTypeEquals) = IsArgumentEqualsToRequested(declarationSymbol, syntax, model, attr.RequestedType);

                    if (!isValidReference)
                        continue;

                    if (isTypeEquals)
                        continue;

                    EditorGUILayout.HelpBox($"The receiver ({currentBehaviour.name}; this) is requesting {attr.RequestedType.FullName}, but the sender is sending other type(s), so it could not be applied.", MessageType.Warning);
                    return;
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldFinalizer(Exception __exception)
        {
            if (__exception is ExitGUIException e)
                throw e;
        }

        private static (bool, RequestArgumentTypeAttribute) ShouldCheckTypeValidation(UdonBehaviour currentBehaviour, string symbol, Type variableType, object @return)
        {
            if (@return == null || variableType != typeof(UdonBehaviour))
                return (false, null);

            if (!UdonSharpEditorUtility.IsUdonSharpBehaviour(currentBehaviour))
                return (false, null);

            if (UdonSharpProgramAsset.GetBehaviourClass((UdonBehaviour) @return) != typeof(EventListener))
                return (false, null);

            var proxyBehaviour = UdonSharpEditorUtility.GetProxyBehaviour(currentBehaviour);
            var variable = proxyBehaviour.GetType().GetField(symbol, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (variable == null)
                return (false, null);

            var attr = variable.GetCustomAttribute<RequestArgumentTypeAttribute>();
            return attr == null ? (false, null) : (true, attr);
        }

        private static List<(UdonBehaviour, string)> CollectListenerReferences(UdonBehaviour currentBehaviour, object @return)
        {
            var callers = new List<(UdonBehaviour, string)>();
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            var components = new List<UdonBehaviour>();

            foreach (var o in rootObjects)
                components.AddRange(o.GetComponentsInChildren<UdonBehaviour>().Where(UdonSharpEditorUtility.IsUdonSharpBehaviour));

            foreach (var behaviour in components)
            {
                var publicVariables = behaviour.publicVariables;
                if (publicVariables == null || publicVariables.VariableSymbols.Count == 0)
                    continue;

                foreach (var s in publicVariables.VariableSymbols)
                    if (publicVariables.TryGetVariableValue(s, out var obj) && obj is Object o && o == (Object) @return && behaviour != currentBehaviour)
                    {
                        callers.Add((behaviour, s));
                        break;
                    }
            }

            return callers;
        }

        private static (SyntaxNode, SemanticModel) CreateAnalysisModels(UdonBehaviour behaviour)
        {
            var source = ((UdonSharpProgramAsset) behaviour.programSource).sourceCsScript.text;
            var syntax = CSharpSyntaxTree.ParseText(source);

            var compilationUnit = CSharpCompilation.Create("Mochizuki.VRChat.Internal")
                                                   .AddReferences(References)
                                                   .AddSyntaxTrees(syntax);

            return (syntax.GetRoot(), compilationUnit.GetSemanticModel(syntax));
        }

        private static IEnumerable<MemberAccessExpressionSyntax> CollectEventListenerArgumentPassingSyntax(SyntaxNode node)
        {
            return node.DescendantNodes().OfType<InvocationExpressionSyntax>().Select(w => w.Expression).Where(w =>
            {
                if (w is MemberAccessExpressionSyntax syntax)
                    return syntax.Name.Identifier.Text == nameof(EventListener.SetArgument);

                return false;
            }).Cast<MemberAccessExpressionSyntax>();
        }

        private static ISymbol FindEventListenerVariableDeclarationSyntax(SyntaxNode node, SemanticModel model, string symbol)
        {
            var declaration = node.DescendantNodes().OfType<FieldDeclarationSyntax>().SelectMany(w => w.Declaration.Variables).First(w => w.Identifier.Text == symbol);
            return model.GetDeclaredSymbol(declaration);
        }

        private static (bool, bool) IsArgumentEqualsToRequested(ISymbol symbol, MemberAccessExpressionSyntax syntax, SemanticModel model, Type t)
        {
            var caller = model.GetSymbolInfo(syntax.Expression);
            if (!symbol.Equals(caller.Symbol))
                return (false, false);

            var invocation = (InvocationExpressionSyntax) syntax.Parent;
            var accessor = (MemberAccessExpressionSyntax) invocation.Expression;
            var identifier = (IdentifierNameSyntax) accessor.Name;
            if (identifier.Identifier.Text != nameof(EventListener.SetArgument))
                return (false, false);

            var argument = invocation.ArgumentList.Arguments[0];
            var declaration = model.GetTypeInfo(argument.Expression);

            return (true, declaration.Type.Equals(model.Compilation.GetTypeByMetadataName(t.FullName)));
        }
    }
}