/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace Mochizuki.VRChat.Interop.Validator.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    [PublicAPI]
    public class SyncedEventAttribute : Attribute { }
}