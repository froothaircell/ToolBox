﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using ToolBox.Utils.Singleton;
using System.Linq;
#if PLATFORM_PS5 || UNITY_PS5 || UNITY_PS5_API
using UnityEngine.PS5;
using ToolBox.Utils;
using PlatformInput = UnityEngine.PS5.PS5Input;
#endif

namespace ToolBox.Managers.InputManagement
{
    public enum ControllerLayout
    {
        KeyboardAndMouse = 0,
        PS5 = 1,
        XBoxSerX = 2
    }
    
    [RequireComponent(typeof(PlayerInput))] // PlayerInput is important for manual control switching
    public class InputManager : MonoSingleton<InputManager>
    {
        private PlayerInput _playerInput;
        private List<InputDevice> _inputDevices;

        private readonly Dictionary<string, ControllerLayout> ControllerLayoutDict = new Dictionary<string, ControllerLayout>()
        {
            {"KeyboardNMouse", ControllerLayout.KeyboardAndMouse },
            {"PS5", ControllerLayout.PS5 },
            {"XBoxSerX", ControllerLayout.XBoxSerX }
        };

        public static Action onControlSchemeChange;
        public static BaseInputActions InputActions { get; private set; }
        public ControllerLayout CurrentControlScheme
        {
            get 
            {
                if (ControllerLayoutDict.TryGetValue(
                        _playerInput.currentControlScheme, 
                        out var controllerLayout))
                    return controllerLayout;
                else
                    throw new MissingReferenceException(
                        $"{_playerInput.currentControlScheme} does not have a corresponding control scheme accounted for!");
            }
        }


#if PLATFORM_PS5 || UNITY_PS5 || UNITY_PS5_API
        // We'll only deal with one player so only take the initial logged in user
        private PlatformInput.LoggedInUser _loggedInUser;

        public PlatformInput.LoggedInUser LoggedInUser
        {
            get { return _loggedInUser; }
        }

        public bool IsConnected
        {
            get { return PlatformInput.PadIsConnected(_loggedInUser.padHandle); }
        }

        private void GetInitialUser()
        {
            var initialID = Utility.initialUserId;
            int slot = PlatformInput.GetSlotFromUserId((uint)initialID);
            _loggedInUser = PlatformInput.GetUsersDetails(slot);
        }

#else
        private bool _keyboardListenersAdded = false;

        public static bool ShiftPressed { get; private set; }
#endif
        public bool IsKeyboardAndMouse
        {
            get
            {
#if PLATFORM_PS5 || UNITY_PS5 || UNITY_PS5_API
                return false;
#else
                return CurrentControlScheme == ControllerLayout.KeyboardAndMouse;
#endif
            }
        }

        #region Overrides
        public override void InitSingleton()
        {
            base.InitSingleton();

            InputActions = new BaseInputActions();
            
            _playerInput = GetComponent<PlayerInput>();
            if (!_playerInput)
                throw new MissingReferenceException($"Player Input component doesn't exist!");

            _inputDevices = new List<InputDevice>(InputSystem.devices);

            InputActions.Player.Enable();
            InputActions.UI.Enable(); // Even if this works maybe we should disable this later
            _playerInput.neverAutoSwitchControlSchemes = true;

            InputSystem.onDeviceChange += OnInputChanged;
            _playerInput.onControlsChanged += OnControlSchemeSwitched;

            PreventDualInputs();

#if PLATFORM_PS5 || UNITY_PS5 || UNITY_PS5_API
            GetInitialUser();
#else
            ShiftPressed = false;
            _keyboardListenersAdded = false;
            CheckForInputs();
#endif
        }

        public override void CleanSingleton()
        {
            InputSystem.onDeviceChange -= OnInputChanged;
            _playerInput.onControlsChanged -= OnControlSchemeSwitched;

            onControlSchemeChange = null;
            InputActions?.Player.Disable();
            InputActions?.UI.Disable();
            InputActions = null;
            _playerInput = null;
            _inputDevices?.Clear();
            _inputDevices = null;
        }

        private void Update()
        {
#if PLATFORM_PS5 || UNITY_PS5 || UNITY_PS5_API

#else
            if (InputActions.Player.Shift.WasReleasedThisFrame())
                ShiftPressed = false;
#endif
        }
        #endregion

        private void OnInputChanged(InputDevice device, InputDeviceChange change)
        {
            // Debug.Log($"Device: {device}, Change: {change}");

#if !PLATFORM_PS5 || !UNITY_PS5 || !UNITY_PS5_API
            if(device.GetType() == typeof(DualSenseGamepadHID) || device.GetType() == typeof(XInputController))
            {
                Debug.Log($"Something happened to gamepad {device}\n Change type: {change}");
            }
#else
            Debug.Log($"Input Manager || PS5 Input Type: {device.GetType()}");
#endif

            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log("New device added: " + device);
                    _inputDevices.Add(device);
                    CheckForInputs();
                   // _playerInput.SwitchCurrentControlScheme(device);
                   
                    break;

                case InputDeviceChange.Disconnected:
                    Debug.Log($"{device} disconnected");
                    CheckForInputs();
                    break;

                case InputDeviceChange.Reconnected:
                    Debug.Log($"{device} reconnected");
                    CheckForInputs();
                    //_playerInput?.SwitchCurrentControlScheme(device);
                    break;

                case InputDeviceChange.Removed:
                    Debug.Log("Device removed: " + device);
                    _inputDevices.Remove(device);
                    if (_inputDevices.Count == 0)
                        throw new MissingReferenceException("No Input Devices Found! Please connect a new input device");
                    CheckForInputs();
                    break;
            }

        }

        public void OnControlSchemeSwitched(PlayerInput currInput)
        {
            Debug.Log($"Player Input Changed to {currInput.currentControlScheme}");
            
            onControlSchemeChange?.Invoke();
            PreventDualInputs();         
        }

        private void PreventDualInputs()
        {
            string bindingGroup = "";

            switch (InputManager.Instance.CurrentControlScheme)
            {   
                case ControllerLayout.KeyboardAndMouse:
                    bindingGroup = InputActions.controlSchemes.First(x => x.name == "KeyboardNMouse").bindingGroup;
                    break;
                case ControllerLayout.PS5:
                    bindingGroup = InputActions.controlSchemes.First(x => x.name == "PS5").bindingGroup;
                    break;
                case ControllerLayout.XBoxSerX:
                    bindingGroup = InputActions.controlSchemes.First(x => x.name == "XBoxSerX").bindingGroup;
                    break;
            }

            InputActions.bindingMask = InputBinding.MaskByGroup(bindingGroup);

        }

        /// <summary>
        /// Checks for inputs, giving a preference 
        /// to Controllers over Keyboard and Mouse
        /// </summary>
        private void CheckForInputs()
        {
#if !PLATFORM_PS5 || !UNITY_PS5 || !UNITY_PS5_API
            if (_inputDevices.Count > 0)
            {
                var xInputIndex = _inputDevices.FindIndex(0, (x) => x is XInputController);
                var dualsenseIndex = _inputDevices.FindIndex(0, (x) => x is DualSenseGamepadHID);
                var keyboardIndex = _inputDevices.FindIndex(0, (x) => x is Keyboard);
                if (dualsenseIndex >= 0)
                {
                    _playerInput.SwitchCurrentControlScheme(_inputDevices[dualsenseIndex]);
                }
                else if (xInputIndex >= 0)
                {
                    _playerInput.SwitchCurrentControlScheme(_inputDevices[xInputIndex]);
                }
                else if (keyboardIndex >= 0)
                {
                    _playerInput.SwitchCurrentControlScheme(
                        _inputDevices[keyboardIndex],
                        _inputDevices.Find(x => x is Mouse));
                }
                else
                {
                    _playerInput.SwitchCurrentControlScheme(
                        _inputDevices[0]);
                }
                AddKeyboardListeners();
            }
            else
                throw new MissingReferenceException("Input device list is empty! Please connect a device");
#else
            foreach (var device in InputSystem.devices)
            {
                _inputDevices.Add(device);
                OnScreenLog.Add(device.ToString());
            }
            _playerInput.SwitchCurrentControlScheme(
                _inputDevices[0]);
#endif
        }

#if !PLATFORM_PS5 || !UNITY_PS5 || !UNITY_PS5_API
        private void AddKeyboardListeners()
        {
            if (IsKeyboardAndMouse)
            {
                InputActions.Player.Shift.performed += OnShiftPressed;
                _keyboardListenersAdded = true;
            }
            else
            {
                if (_keyboardListenersAdded)
                    InputActions.Player.Shift.performed -= OnShiftPressed;
                ShiftPressed = true; // this remains true if the keyboard isn't working
                _keyboardListenersAdded = false;
            }
        }

        private void OnShiftPressed(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                ShiftPressed = true;
            }
        }
#endif
    }
}