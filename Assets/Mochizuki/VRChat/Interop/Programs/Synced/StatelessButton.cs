/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UnityEngine;

using VRC.Udon.Common.Interfaces;

#pragma warning disable CS0649

namespace Mochizuki.VRChat.Interop.Synced
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class StatelessButton : UdonSharpBehaviour
    {
        [SerializeField]
        [SyncedEvent]
        private EventListener listener;

        [SerializeField]
        private InteropLogger logger;

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
    }
}