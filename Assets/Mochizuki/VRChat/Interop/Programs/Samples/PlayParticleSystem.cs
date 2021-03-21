/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

using UnityEngine;

namespace Mochizuki.VRChat.Interop.Samples
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PlayParticleSystem : UdonSharpBehaviour
    {
        private void Start()
        {
            particle.Stop(true);
        }

        private void Update()
        {
            if (!listener.IsInteracted())
                return;

            particle.Play(true);
        }

#pragma warning disable CS0649

        [SerializeField]
        private EventListener listener;

        [SerializeField]
        private ParticleSystem particle;

#pragma warning restore CS0649
    }
}