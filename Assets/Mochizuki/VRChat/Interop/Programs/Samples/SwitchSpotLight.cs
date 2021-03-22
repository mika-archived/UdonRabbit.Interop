/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UnityEngine;

#pragma warning disable CS0649

namespace Mochizuki.VRChat.Interop.Samples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class SwitchSpotLight : UdonSharpBehaviour
    {
        [SerializeField]
        [RequestArgumentType(typeof(bool))]
        [RequestSyncedEvent]
        private EventListener listener;

        [SerializeField]
        private Light lighting;

        private void Start()
        {
            lighting.enabled = false;
        }

        private void Update()
        {
            if (!listener.IsInteracted())
                return;

            lighting.enabled = (bool) listener.GetArgument();
        }
    }
}