/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Linq;

using UnityEditor;

namespace Mochizuki.VRChat.Interop
{
    [InitializeOnLoad]
    public static class InteropModeEnabler
    {
        private const string Preprocessor = "MOCHIZUKI_INTEROP";

        static InteropModeEnabler()
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').ToList();
            if (symbols.Contains(Preprocessor))
                return;

            symbols.Add(Preprocessor);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", symbols));
        }
    }
}