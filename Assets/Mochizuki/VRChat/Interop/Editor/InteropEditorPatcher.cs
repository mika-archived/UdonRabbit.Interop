﻿/*-------------------------------------------------------------------------------------------
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

        private static readonly Type[] Validators =
        {
            typeof(RequestArgumentTypeAttribute),
            typeof(RequestNoSyncedEventAttribute),
            typeof(RequestSyncedEventAttribute)
        };

        static InteropEditorPatcher()
        {
            References = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // mscorlib
                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location), // UnityEngine
                MetadataReference.CreateFromFile(typeof(Editor).Assembly.Location), // UnityEditor
                MetadataReference.CreateFromFile(typeof(UdonBehaviour).Assembly.Location), // VRC.Udon
                MetadataReference.CreateFromFile(typeof(UdonSharpBehaviour).Assembly.Location), // UdonSharp
                MetadataReference.CreateFromFile(typeof(EventListener).Assembly.Location), // Mochizuki.VRChat.Interop
                MetadataReference.CreateFromFile(typeof(RequestArgumentTypeAttribute).Assembly.Location) // Mochizuki.VRChat.Interop.Validator
            };

            var harmony = new Harmony("moe.mochizuki.vrchat.interop");

            ApplyPatchForDrawPublicVariableField(harmony);
        }

        private static void ApplyPatchForDrawPublicVariableField(Harmony harmony)
        {
            // patched to https://github.com/MerlinVR/UdonSharp/blob/v0.19.6/Assets/UdonSharp/Editor/Editors/UdonSharpGUI.cs#L1050
            var original = typeof(UdonSharpGUI).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).FirstOrDefault(w => w.Name == "DrawPublicVariableField");
            var postfix = typeof(InteropEditorPatcher).GetMethod(nameof(DrawPublicVariableFieldPostfix));
            var finalizer = typeof(InteropEditorPatcher).GetMethod(nameof(DrawPublicVariableFieldFinalizer));

            harmony.Patch(original, null, new HarmonyMethod(postfix), null, new HarmonyMethod(finalizer));
        }

        [PublicAPI]
        public static void RegisterReferences(Type t)
        {
            References.AddItem(MetadataReference.CreateFromFile(t.Assembly.Location));
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldPostfix(UdonBehaviour currentBehaviour, string symbol, Type variableType, ref object __result)
        {
            var (r, attrs) = ShouldRunValidator(currentBehaviour, symbol, variableType, __result);
            if (!r)
                return;

            var callers = CollectListenerReferences(currentBehaviour, __result);

            foreach (var (behaviour, refSymbol) in callers)
            {
                var (root, model) = CreateAnalysisModels(behaviour);
                var argumentPasser = CollectEventListenerArgumentPassingSyntax(root);
                var declarationSymbol = FindEventListenerVariableDeclarationSyntax(root, model, refSymbol);
                var hasWarnings = false;

                if (attrs.OfType<RequestArgumentTypeAttribute>().Any())
                {
                    var attr = attrs.OfType<RequestArgumentTypeAttribute>().First();
                    if (!IsArgumentEqualsToRequested(declarationSymbol, argumentPasser, model, attr.RequestedType))
                    {
                        EditorGUILayout.HelpBox($"The receiver ({currentBehaviour.name}; this) is requesting {attr.RequestedType.FullName}, but the one or more sender is sending other type(s), so it could not be applied.", MessageType.Warning);
                        hasWarnings = true;
                    }
                }

                if (attrs.OfType<RequestSyncedEventAttribute>().Any() && !IsEventListenerEqualsToRequested(declarationSymbol, true))
                {
                    EditorGUILayout.HelpBox($"The receiver ({currentBehaviour.name}; this) is requesting `Synced`, but the one or more sender is sending `NoSynced` or `Any`, so it could not be applied.", MessageType.Warning);
                    hasWarnings = true;
                }

                if (attrs.OfType<RequestNoSyncedEventAttribute>().Any() && !IsEventListenerEqualsToRequested(declarationSymbol, false))
                {
                    EditorGUILayout.HelpBox($"The receiver ({currentBehaviour.name}; this) is requesting `NoSynced`, but the one or more sender is sending `Synced` or `Any`, so it could not be applied.", MessageType.Warning);
                    hasWarnings = true;
                }

                if (hasWarnings)
                    return;
            }
        }

        // ReSharper disable once InconsistentNaming
        public static void DrawPublicVariableFieldFinalizer(Exception __exception)
        {
            if (__exception is ExitGUIException e)
                throw e;
        }

        private static (bool, List<Attribute>) ShouldRunValidator(UdonBehaviour currentBehaviour, string symbol, Type variableType, object @return)
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

            var attrs = Validators.Select(validator => variable.GetCustomAttribute(validator)).Where(attr => attr != null).ToList();
            return attrs.Count == 0 ? (false, null) : (true, attrs);
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

        private static bool IsArgumentEqualsToRequested(ISymbol symbol, IEnumerable<MemberAccessExpressionSyntax> callers, SemanticModel model, Type t)
        {
            foreach (var syntax in callers)
            {
                var caller = model.GetSymbolInfo(syntax.Expression);
                if (!symbol.Equals(caller.Symbol))
                    continue;

                var invocation = (InvocationExpressionSyntax) syntax.Parent;
                var accessor = (MemberAccessExpressionSyntax) invocation.Expression;
                var identifier = (IdentifierNameSyntax) accessor.Name;
                if (identifier.Identifier.Text != nameof(EventListener.SetArgument))
                    continue;

                var argument = invocation.ArgumentList.Arguments[0];
                var declaration = model.GetTypeInfo(argument.Expression);

                if (!declaration.Type.Equals(model.Compilation.GetTypeByMetadataName(t.FullName)))
                    return false;
            }

            return true;
        }

        private static bool IsEventListenerEqualsToRequested(ISymbol symbol, bool requestSynced)
        {
            var attrs = symbol.GetAttributes().Select(w => w.AttributeClass?.ToDisplayString()).ToList();
            var isSynced = attrs.Any(w => w == typeof(SyncedEventAttribute).FullName);
            var isNoSynced = attrs.Any(w => w == typeof(NoSyncedEventAttribute).FullName);

            if (!requestSynced)
                return !isSynced && isNoSynced;
            if (isSynced)
                return true;
            return !isNoSynced;
        }
    }
}