/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using TMPro;

using UdonSharp;

using UnityEngine;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class DebugConsole : UdonSharpBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI console;

        [SerializeField]
        private int maxLines;

        private bool _hasConsole;
        private int _len;
        private string[] _lines;
        private int _ptr;

        private void Start()
        {
            _hasConsole = console != null;
            _lines = new string[maxLines];
        }

        public void AddLine(string line)
        {
            _lines[_ptr++] = $"{line}\n";
            _len++;

            if (_ptr >= maxLines)
                _ptr = 0;
            if (_len >= maxLines)
                _len = maxLines;

            var text = "";
            for (var i = 0; i < _len; i++)
                text += _lines[(maxLines + _ptr - _len + i) % maxLines];

            if (_hasConsole)
                console.text = text;
        }

        public void Clear()
        {
            _len = 0;
            _ptr = 0;

            for (var i = 0; i < maxLines; i++)
                _lines[i] = "";

            if (_hasConsole)
                console.text = "";
        }
    }
}