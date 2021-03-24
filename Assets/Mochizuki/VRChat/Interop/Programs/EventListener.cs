/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using UdonSharp;

using UnityEngine;

using VRC.SDK3.Components.Video;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace Mochizuki.VRChat.Interop
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    [DefaultExecutionOrder(10000000)]
    public class EventListener : UdonSharpBehaviour
    {
        // InputJump, InputUse, InputGrab, InputDrop
        // InputMoveHorizontal, InputMoveVertical, InputLookHorizontal, InputLookVertical
        private UdonInputEventArgs _args;

        // MidiNoteOn, MidiNoteOff, MidiControlChange
        private int _channel;

        // InputJump, InputUse, InputGrab, InputDrop
        private bool _inputValueB;

        // InputMoveHorizontal, InputMoveVertical, InputLookHorizontal, InputLookVertical
        private float _inputValueF;

        // Is Some Event Is Emitted
        private bool _isEmitted;

        // MidiControlChange
        private int _midiChangeValue;

        // MidiNoteOn, MidiNoteOff, MidiControlChange
        private int _number;

        // Extra Argument
        private object _object;

        // OnPlayerJoined, OnPlayerLeft, OnStationEntered, OnStationExited
        // OnPlayerTriggerEnter, OnPlayerTriggerExit, OnPlayerTriggerStay
        // OnPlayerCollisionEnter, OnPlayerCollisionExit, OnPlayerCollisionStay
        // OnPlayerParticleCollision, OnPlayerRespawn
        private VRCPlayerApi _player;

        // OnOwnershipRequest
        private VRCPlayerApi _requestedOwner;

        // MidiNoteOn, MidiNoteOff
        private int _velocity;

        // OnVideoError
        private VideoError _videoError;

        private void Update()
        {
            // reset event
            if (!_isEmitted)
                return;

            _isEmitted = false;
            _isInteract = false;
            _isDrop = false;
            _isPickup = false;
            _isPickupUseDown = false;
            _isPickupUseUp = false;
            _isPlayerJoined = false;
            _isPlayerLeft = false;
            _isSpawn = false;
            _isStationEntered = false;
            _isStationExited = false;
            _isVideoEnd = false;
            _isVideoError = false;
            _isVideoLoop = false;
            _isVideoPause = false;
            _isVideoPlay = false;
            _isVideoReady = false;
            _isVideoStart = false;
            _isPlayerTriggerEnter = false;
            _isPlayerTriggerExit = false;
            _isPlayerTriggerStay = false;
            _isPlayerCollisionEnter = false;
            _isPlayerCollisionExit = false;
            _isPlayerCollisionStay = false;
            _isPlayerParticleCollision = false;
            _isPlayerRespawn = false;
            _isOwnershipRequest = false;
            _isMidiNoteOn = false;
            _isMidiNoteOff = false;
            _isMidiControlChange = false;
            _isInputJump = false;
            _isInputUse = false;
            _isInputGrab = false;
            _isInputDrop = false;
            _isInputMoveHorizontal = false;
            _isInputMoveVertical = false;
            _isInputLookHorizontal = false;
            _isInputLookVertical = false;
        }

        public bool IsSomeEventIsEmitted()
        {
            return _isEmitted;
        }

        public UdonInputEventArgs GetUdonInputEventArgs()
        {
            return _args;
        }

        public bool GetInputValueB()
        {
            return _inputValueB;
        }

        public float GetInputValueF()
        {
            return _inputValueF;
        }

        public int GetMidiChannelArg()
        {
            return _channel;
        }

        public int GetMidiNumberArg()
        {
            return _number;
        }

        public int GetMidiVelocityArg()
        {
            return _velocity;
        }

        public int GetMidiChangeValueArg()
        {
            return _midiChangeValue;
        }

        public VRCPlayerApi GetPlayerEventArg()
        {
            return _player;
        }

        // alias for OwnershipRequest
        public VRCPlayerApi GetRequestingPlayerArg()
        {
            return _player;
        }

        public VRCPlayerApi GetRequestedOwnerArg()
        {
            return _requestedOwner;
        }

        public VideoError GetVideoErrorEventArg()
        {
            return _videoError;
        }

        #region EmitInteracted

        private bool _isInteract;

        public void EmitInteract()
        {
            _isEmitted = _isInteract = true;
        }

        public bool IsInteract()
        {
            return _isInteract;
        }

        #endregion

        #region EmitDrop

        private bool _isDrop;

        public void EmitDrop()
        {
            _isEmitted = _isDrop = true;
        }

        public bool IsDrop()
        {
            return _isDrop;
        }

        #endregion

        #region EmitOwnershipTransferred

        private bool _isOwnershipTransferred;

        public void EmitOwnershipTransferred(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isOwnershipTransferred = true;
        }

        public bool IsOwnershipTransferred()
        {
            return _isOwnershipTransferred;
        }

        #endregion

        #region EmitPickup

        private bool _isPickup;

        public void EmitPickup()
        {
            _isEmitted = _isPickup = true;
        }

        public bool IsPickup()
        {
            return _isPickup;
        }

        #endregion

        #region EmitPickupUseDown

        private bool _isPickupUseDown;

        public void EmitPickupUseDown()
        {
            _isEmitted = _isPickupUseDown = true;
        }

        public bool IsPickupUseDown()
        {
            return _isPickupUseDown;
        }

        #endregion

        #region EmitPickupUseUp

        private bool _isPickupUseUp;

        public void EmitPickupUseUp()
        {
            _isEmitted = _isPickupUseUp = true;
        }

        public bool IsPickupUseUp()
        {
            return _isPickupUseUp;
        }

        #endregion

        #region EmitPlayerJoined

        private bool _isPlayerJoined;

        public void EmitPlayerJoined(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerJoined = true;
        }

        public bool IsPlayerJoined()
        {
            return _isPlayerJoined;
        }

        #endregion

        #region EmitPlayerLeft

        private bool _isPlayerLeft;

        public void EmitPlayerLeft(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerLeft = true;
        }

        public bool IsPlayerLeft()
        {
            return _isPlayerLeft;
        }

        #endregion

        #region EmitSpawn

        private bool _isSpawn;

        public void EmitSpawn()
        {
            _isEmitted = _isSpawn = true;
        }

        public bool IsSpawn()
        {
            return _isSpawn;
        }

        #endregion

        #region EmitStationEntered

        private bool _isStationEntered;

        public void EmitStationEntered(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isStationEntered = true;
        }

        public bool IsStationEntered()
        {
            return _isStationEntered;
        }

        #endregion

        #region EmitStationExited

        private bool _isStationExited;

        public void EmitStationExited(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isStationExited = true;
        }

        public bool IsStationExited()
        {
            return _isStationExited;
        }

        #endregion

        #region EmitVideoEnd

        private bool _isVideoEnd;

        public void EmitVideoEnd()
        {
            _isEmitted = _isVideoEnd = true;
        }

        public bool IsVideoEnd()
        {
            return _isVideoEnd;
        }

        #endregion

        #region EmitVideoError

        private bool _isVideoError;

        public void EmitVideoError(VideoError videoError)
        {
            _videoError = videoError;
            _isEmitted = _isVideoError = true;
        }

        public bool IsVideoError()
        {
            return _isVideoError;
        }

        #endregion

        #region EmitVideoLoop

        private bool _isVideoLoop;

        public void EmitVideoLoop()
        {
            _isEmitted = _isVideoLoop = true;
        }

        public bool IsVideoLoop()
        {
            return _isVideoLoop;
        }

        #endregion

        #region EmitVideoPause

        private bool _isVideoPause;

        public void EmitVideoPause()
        {
            _isEmitted = _isVideoPause = true;
        }

        public bool IsVideoPause()
        {
            return _isVideoPause;
        }

        #endregion

        #region EmitVideoPlay

        private bool _isVideoPlay;

        public void EmitVideoPlay()
        {
            _isEmitted = _isVideoPlay = true;
        }

        public bool IsVideoPlay()
        {
            return _isVideoPlay;
        }

        #endregion

        #region EmitVideoReady

        private bool _isVideoReady;

        public void EmitVideoReady()
        {
            _isEmitted = _isVideoReady = true;
        }

        public bool IsVideoReady()
        {
            return _isVideoReady;
        }

        #endregion

        #region EmitVideoStart

        private bool _isVideoStart;

        public void EmitVideoStart()
        {
            _isEmitted = _isVideoStart = true;
        }

        public bool IsVideoStart()
        {
            return _isVideoStart;
        }

        #endregion

        #region EmitPlayerTriggerEnter

        private bool _isPlayerTriggerEnter;

        public void EmitPlayerTriggerEnter(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerTriggerEnter = true;
        }

        public bool IsPlayerTriggerEnter()
        {
            return _isPlayerTriggerEnter;
        }

        #endregion

        #region EmitPlayerTriggerExit

        private bool _isPlayerTriggerExit;

        public void EmitPlayerTriggerExit(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerTriggerExit = true;
        }

        public bool IsPlayerTriggerExit()
        {
            return _isPlayerTriggerExit;
        }

        #endregion

        #region EmitPlayerTriggerStay

        private bool _isPlayerTriggerStay;

        public void EmitPlayerTriggerStay(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerTriggerStay = true;
        }

        public bool IsPlayerTriggerStay()
        {
            return _isPlayerTriggerStay;
        }

        #endregion

        #region EmitPlayerCollisionEnter

        private bool _isPlayerCollisionEnter;

        public void EmitPlayerCollisionEnter(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerCollisionEnter = true;
        }

        public bool IsPlayerCollisionEnter()
        {
            return _isPlayerCollisionEnter;
        }

        #endregion

        #region EmitPlayerCollisionExit

        private bool _isPlayerCollisionExit;

        public void EmitPlayerCollisionExit(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerCollisionExit = true;
        }

        public bool IsPlayerCollisionExit()
        {
            return _isPlayerCollisionExit;
        }

        #endregion

        #region EmitPlayerCollisionStay

        private bool _isPlayerCollisionStay;

        public void EmitPlayerCollisionStay(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerCollisionStay = true;
        }

        public bool IsPlayerCollisionStay()
        {
            return _isPlayerCollisionStay;
        }

        #endregion

        #region EmitPlayerParticleCollision

        private bool _isPlayerParticleCollision;

        public void EmitPlayerParticleCollision(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerParticleCollision = true;
        }

        public bool IsPlayerParticleCollision()
        {
            return _isPlayerParticleCollision;
        }

        #endregion

        #region EmitPlayerRespawn

        private bool _isPlayerRespawn;

        public void EmitPlayerRespawn(VRCPlayerApi player)
        {
            _player = player;
            _isEmitted = _isPlayerRespawn = true;
        }

        public bool IsPlayerRespawn()
        {
            return _isPlayerRespawn;
        }

        #endregion

        #region EmitOwnershipRequest

        private bool _isOwnershipRequest;

        public void EmitOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            _player = requestingPlayer;
            _requestedOwner = requestedOwner;
            _isEmitted = _isOwnershipRequest = true;
        }

        public bool IsOwnershipRequest()
        {
            return _isOwnershipRequest;
        }

        #endregion

        #region EmitMidiNoteOn

        private bool _isMidiNoteOn;

        public void EmitMidiNoteOn(int channel, int number, int velocity)
        {
            _channel = channel;
            _number = number;
            _velocity = velocity;
            _isEmitted = _isMidiNoteOn = true;
        }

        public bool IsMidiNoteOn()
        {
            return _isMidiNoteOn;
        }

        #endregion

        #region EmitMidiNoteOff

        private bool _isMidiNoteOff;

        public void EmitMidiNoteOff(int channel, int number, int velocity)
        {
            _channel = channel;
            _number = number;
            _velocity = velocity;
            _isEmitted = _isMidiNoteOff = true;
        }

        public bool IsMidiNoteOff()
        {
            return _isMidiNoteOff;
        }

        #endregion

        #region EmitMidiControlChange

        private bool _isMidiControlChange;

        public void EmitMidiControlChange(int channel, int number, int value)
        {
            _channel = channel;
            _number = number;
            _midiChangeValue = value;
            _isEmitted = _isMidiControlChange = true;
        }

        public bool IsMidiControlChange()
        {
            return _isMidiControlChange;
        }

        #endregion

        #region EmitInputJump

        private bool _isInputJump;

        public void EmitInputJump(bool value, UdonInputEventArgs args)
        {
            _inputValueB = value;
            _args = args;
            _isEmitted = _isInputJump = true;
        }

        public bool IsInputJump()
        {
            return _isInputJump;
        }

        #endregion

        #region EmitInputUse

        private bool _isInputUse;

        public void EmitInputUse(bool value, UdonInputEventArgs args)
        {
            _inputValueB = value;
            _args = args;
            _isEmitted = _isInputUse = true;
        }

        public bool IsInputUse()
        {
            return _isInputUse;
        }

        #endregion

        #region EmitInputGrab

        private bool _isInputGrab;

        public void EmitInputGrab(bool value, UdonInputEventArgs args)
        {
            _inputValueB = value;
            _args = args;
            _isEmitted = _isInputGrab = true;
        }

        public bool IsInputGrab()
        {
            return _isInputGrab;
        }

        #endregion

        #region EmitInputDrop

        private bool _isInputDrop;

        public void EmitInputDrop(bool value, UdonInputEventArgs args)
        {
            _inputValueB = value;
            _args = args;
            _isEmitted = _isInputDrop = true;
        }

        public bool IsInputDrop()
        {
            return _isInputDrop;
        }

        #endregion

        #region EmitInputMoveHorizontal

        private bool _isInputMoveHorizontal;

        public void EmitInputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            _inputValueF = value;
            _args = args;
            _isEmitted = _isInputMoveHorizontal = true;
        }

        public bool IsInputMoveHorizontal()
        {
            return _isInputMoveHorizontal;
        }

        #endregion

        #region EmitInputMoveVertical

        private bool _isInputMoveVertical;

        public void EmitInputMoveVertical(float value, UdonInputEventArgs args)
        {
            _inputValueF = value;
            _args = args;
            _isEmitted = _isInputMoveVertical = true;
        }

        public bool IsInputMoveVertical()
        {
            return _isInputMoveVertical;
        }

        #endregion

        #region EmitInputLookHorizontal

        private bool _isInputLookHorizontal;

        public void EmitInputLookHorizontal(float value, UdonInputEventArgs args)
        {
            _inputValueF = value;
            _args = args;
            _isEmitted = _isInputLookHorizontal = true;
        }

        public bool IsInputLookHorizontal()
        {
            return _isInputLookHorizontal;
        }

        #endregion

        #region EmitInputLookVertical

        private bool _isInputLookVertical;

        public void EmitInputLookVertical(float value, UdonInputEventArgs args)
        {
            _inputValueF = value;
            _args = args;
            _isEmitted = _isInputLookVertical = true;
        }

        public bool IsInputLookVertical()
        {
            return _isInputLookVertical;
        }

        #endregion

        #region Extra Argument(s)

        public void SetArgument(object obj)
        {
            _object = obj;
        }

        public object GetArgument()
        {
            return _object;
        }

        #endregion
    }
}