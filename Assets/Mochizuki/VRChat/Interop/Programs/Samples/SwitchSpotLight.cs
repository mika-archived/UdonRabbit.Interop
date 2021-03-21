/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UnityEngine;

namespace Mochizuki.VRChat.Interop.Samples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class SwitchSpotLight : UdonSharpBehaviour
    {
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

#pragma warning disable CS0649

        [SerializeField]
        [RequestArgumentType(typeof(bool))]
        private EventListener listener;

        [SerializeField]
        private Light lighting;

#pragma warning restore CS0649
    }
}