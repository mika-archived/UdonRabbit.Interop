/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UnityEngine;

#pragma warning disable CS0649

namespace Mochizuki.VRChat.Interop.NoSynced
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class KitsunePickup : UdonSharpBehaviour
    {
        [SerializeField]
        [NoSyncedEvent]
        public EventListener listener;

        [SerializeField]
        private InteropLogger logger;

        [SerializeField]
        private long respawnAfter;

        private long _droppedAt;
        private bool _hasListener;
        private bool _hasLogger;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private void Start()
        {
            _droppedAt = 0;
            _hasListener = listener != null;
            _hasLogger = logger != null;

            var t = transform;
            _initialPosition = t.localPosition;
            _initialRotation = t.localRotation;
        }

        private void Update()
        {
            if (_droppedAt > 0 && _droppedAt + respawnAfter <= GetUnixTimeMills())
            {
                if (_hasLogger)
                    logger.LogInfo($"The object was dropped {respawnAfter / 1000} seconds ago, so it respawn. ");

                _droppedAt = 0;

                var t = transform;
                t.localPosition = _initialPosition;
                t.localRotation = _initialRotation;
            }
        }

        public override void OnPickup()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnPickup)} - event fired but worked only local");

            if (_hasListener)
                listener.OnPickupped();
        }

        public override void OnPickupUseDown()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnPickupUseDown)} - event fired but worked only local");

            if (_hasListener)
                listener.OnPickupUseDowned();
        }

        public override void OnPickupUseUp()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnPickupUseUp)} - event fired but worked only local");

            if (_hasListener)
                listener.OnPickupUseUpped();
        }

        public override void OnDrop()
        {
            if (_hasLogger)
                logger.LogInfo($"{nameof(OnDrop)} - event fired but worked only local");

            if (_hasListener)
                listener.OnDropped();

            _droppedAt = GetUnixTimeMills();
        }

        private long GetUnixTimeMills()
        {
            return new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0)).ToUnixTimeMilliseconds();
        }
    }
}