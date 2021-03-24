/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.VRChat.Interop.Validator.Attributes;

using UdonSharp;

using UnityEngine;

#pragma warning disable CS0649

namespace Mochizuki.VRChat.Interop.ReceiverSamples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(20000)]
    public class PlayParticleSystem : UdonSharpBehaviour
    {
        [SerializeField]
        [RequestValidateEvent]
        private EventListener listener;

        [SerializeField]
        private ParticleSystem particle;

        private void Start()
        {
            particle.Stop(true);
        }

        private void Update()
        {
            if (!listener.IsInteract())
                return;

            particle.Play(true);
        }
    }
}