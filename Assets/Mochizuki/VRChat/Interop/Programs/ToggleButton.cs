/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

using UnityEngine;

using VRC.SDKBase;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ToggleButton : UdonSharpBehaviour
    {
        [SerializeField]
        private bool initialState;

        [SerializeField]
        private EventListener listener;

        [SerializeField]
        private InteropLogger logger;

        private bool _hasListener;
        private bool _hasLogger;

        [UdonSynced]
        private bool _state;

        private void Start()
        {
            _state = initialState;
            _hasListener = listener != null;
            _hasLogger = logger != null;
        }

        public override void Interact()
        {
            if (!_hasListener)
                return;

            if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);

            if (_hasLogger)
                logger.LogInfo($"{nameof(Interact)} - Request Manual Synchronization");

            _state = !_state;

            // for owner
            if (_hasListener)
            {
                listener.SetArgument(_state);
                listener.OnInteracted();
            }

            RequestSerialization(); // for other users
        }

        public override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnOwnershipRequest)} - Request Transfer Ownership to {requestedOwner.displayName} - Approved");
            return true;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnOwnershipTransferred)} - Ownership is transferred to {player.displayName}");
        }

        public override void OnPreSerialization()
        {
            if (_hasLogger)
            {
                logger.LogInfo($"{nameof(OnPreSerialization)} - Request Serialization Variables for Synchronization");
                logger.LogInfo($"{nameof(OnPreSerialization)} - Current State is {!_state}");
            }
        }

        public override void OnDeserialization()
        {
            if (_hasLogger)
            {
                logger.LogInfo($"{nameof(OnDeserialization)} - Received Serialization Variables for Synchronization");
                logger.LogInfo($"{nameof(OnDeserialization)} - Current State is {_state}");
            }

            if (_hasListener)
            {
                listener.SetArgument(_state);
                listener.OnInteracted();
            }
        }
    }
}