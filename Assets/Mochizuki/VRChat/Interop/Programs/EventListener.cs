/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class EventListener : UdonSharpBehaviour
    {
        private object _object;

        public void SetArgument(object obj)
        {
            _object = obj;
        }

        public object GetArgument()
        {
            return _object;
        }

        public bool IsSomeEventIsFired()
        {
            return IsPickupped() || IsDropped() || IsPickupped() || IsPickupUseDowned() || IsPickupUseUpped();
        }

        #region OnInteracted

        private bool _isInteracted;

        public void OnInteracted()
        {
            _isInteracted = true;
        }

        public bool IsInteracted()
        {
            if (!_isInteracted)
                return false;

            _isInteracted = false;
            return true;
        }

        #endregion

        #region OnDropped

        private bool _isDropped;

        public void OnDropped()
        {
            _isDropped = true;
        }

        public bool IsDropped()
        {
            if (!_isDropped)
                return false;

            _isDropped = false;
            return true;
        }

        #endregion

        #region OnPickupped

        private bool _isPickupped;

        public void OnPickupped()
        {
            _isPickupped = true;
        }

        public bool IsPickupped()
        {
            if (!_isPickupped)
                return false;

            _isPickupped = false;
            return true;
        }

        #endregion

        #region OnPickupUseDowned

        private bool _isPickupUseDowned;

        public void OnPickupUseDowned()
        {
            _isPickupUseDowned = true;
        }

        public bool IsPickupUseDowned()
        {
            if (!_isPickupUseDowned)
                return false;

            _isPickupUseDowned = false;
            return true;
        }

        #endregion

        #region OnPickupUseUpped

        private bool _isPickupUseUpped;

        public void OnPickupUseUpped()
        {
            _isPickupUseUpped = true;
        }

        public bool IsPickupUseUpped()
        {
            if (!_isPickupUseUpped)
                return false;

            _isPickupUseUpped = false;
            return true;
        }

        #endregion
    }
}