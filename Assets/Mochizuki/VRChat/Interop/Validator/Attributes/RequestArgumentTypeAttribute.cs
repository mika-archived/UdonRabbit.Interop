/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;

namespace Mochizuki.VRChat.Interop.Validator.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class RequestArgumentTypeAttribute : Attribute
    {
        public Type RequestedType { get; }

        public RequestArgumentTypeAttribute(Type t)
        {
            RequestedType = t;
        }
    }
}