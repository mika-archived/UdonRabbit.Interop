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
    public class SwitchGameObject : UdonSharpBehaviour
    {
        [SerializeField]
        [RequestArgumentType(typeof(bool))]
        [RequestValidateEvent]
        private EventListener listener;

        [SerializeField]
        private GameObject go;

        private void Start()
        {
            go.SetActive((bool) listener.GetArgument());
        }

        private void Update()
        {
            if (!listener.IsInteract())
                return;

            go.SetActive((bool) listener.GetArgument());
        }
    }
}