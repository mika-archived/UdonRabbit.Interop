/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class StatelessButton : UdonSharpBehaviour
    {
        private bool _hasListener;

        private bool _hasLogger;

        private void Start()
        {
            _hasListener = listener != null;
            _hasLogger = logger != null;
        }

        public override void Interact()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(Interact)} - {nameof(SendCustomNetworkEvent)}({nameof(OnInteracted)})");
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnInteracted));
        }

        public void OnInteracted()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnInteracted)} - Event Called");

            if (_hasListener)
                listener.OnInteracted();
        }

#pragma warning disable CS0649

        [SerializeField]
        private EventListener listener;

        [SerializeField]
        private InteropLogger logger;

#pragma warning restore CS0649
    }
}