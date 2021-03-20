/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;

using UdonSharp;

using UnityEngine;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class InteropLogger : UdonSharpBehaviour
    {
        [SerializeField]
        private DebugConsole console;

        [SerializeField]
        private string @namespace;

        private bool _hasConsole;

        private void Start()
        {
            _hasConsole = console != null;
        }

        public void LogInfo(string str)
        {
            Debug.Log($"[{@namespace}] [INFO] {str}");
            if (_hasConsole)
                console.AddLine($"[{DateTime.Now:T}] [<color=#00b8d4>INFO</color>] {str}");
        }

        public void LogWarning(string str)
        {
            Debug.LogWarning($"[{@namespace}] [WARN] {str}");
            if (_hasConsole)
                console.AddLine($"[{DateTime.Now:T}] [<color=#ff9100>WARN</color>] {str}");
        }

        public void LogError(string str)
        {
            Debug.LogError($"[{@namespace}] [ERROR] {str}");
            if (_hasConsole)
                console.AddLine($"[{DateTime.Now:T}] [<color=#ff1744>ERROR</color>] {str}");
        }
    }
}