using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

using Joystick = Iztar.InputSystem.Joystick;

namespace Iztar.Manager
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Custom Joystick (Mobile)")]
        [SerializeField] private Joystick leftJoystick;
        [SerializeField] private Joystick rightJoystick;

        private ShipInputActions shipInputActions;

        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 systemMovementInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 leftJoystickInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 systemAimInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 rightJoystickInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private bool dashPressed;

        // Delegate references
        private Action<InputAction.CallbackContext> movePerformedHandler;
        private Action<InputAction.CallbackContext> moveCanceledHandler;
        private Action<InputAction.CallbackContext> dashPerformedHandler;
        private Action<InputAction.CallbackContext> aimPerformedHandler;
        private Action<InputAction.CallbackContext> aimCanceledHandler;

        private Action<Vector2> leftJoystickHandler;
        private Action<Vector2> rightJoystickHandler;

        // Events
        public event Action<Vector2> OnAimInputPerformed;
        public event Action OnAimInputCancelled;

        private void Awake()
        {
            SetupSingleton();
            shipInputActions = new ShipInputActions();

            // Input System handlers
            movePerformedHandler = ctx => systemMovementInput = ctx.ReadValue<Vector2>();
            moveCanceledHandler = ctx => systemMovementInput = Vector2.zero;
            dashPerformedHandler = ctx => dashPressed = true;

            aimPerformedHandler = ctx =>
            {
                systemAimInput = ctx.ReadValue<Vector2>();
                OnAimInputPerformed?.Invoke(systemAimInput);
            };
            aimCanceledHandler = ctx =>
            {
                systemAimInput = Vector2.zero;
                OnAimInputCancelled?.Invoke();
            };

            // Joystick handlers
            leftJoystickHandler = val => leftJoystickInput = val;

            rightJoystickHandler = val =>
            {
                rightJoystickInput = val;
                if (val.sqrMagnitude > 0.01f)
                    OnAimInputPerformed?.Invoke(val);
                else
                    OnAimInputCancelled?.Invoke();
            };
        }

        private void OnEnable()
        {
            shipInputActions.Enable();

            shipInputActions.ShipController.Move.performed += movePerformedHandler;
            shipInputActions.ShipController.Move.canceled += moveCanceledHandler;
            shipInputActions.ShipController.Dash.performed += dashPerformedHandler;
            shipInputActions.ShipController.Aim.performed += aimPerformedHandler;
            shipInputActions.ShipController.Aim.canceled += aimCanceledHandler;

            if (leftJoystick != null) leftJoystick.OnDirectionChanged += leftJoystickHandler;
            if (rightJoystick != null) rightJoystick.OnDirectionChanged += rightJoystickHandler;
        }

        private void OnDisable()
        {
            shipInputActions.ShipController.Move.performed -= movePerformedHandler;
            shipInputActions.ShipController.Move.canceled -= moveCanceledHandler;
            shipInputActions.ShipController.Dash.performed -= dashPerformedHandler;
            shipInputActions.ShipController.Aim.performed -= aimPerformedHandler;
            shipInputActions.ShipController.Aim.canceled -= aimCanceledHandler;

            if (leftJoystick != null) leftJoystick.OnDirectionChanged -= leftJoystickHandler;
            if (rightJoystick != null) rightJoystick.OnDirectionChanged -= rightJoystickHandler;

            shipInputActions.Disable();
        }

        private void SetupSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // Public API
        public Vector2 GetMoveInput()
        {
            Vector2 combined = systemMovementInput;

            if (leftJoystickInput.sqrMagnitude > 0.01f)
                combined += leftJoystickInput;

            if (combined.sqrMagnitude > 1f)
                combined.Normalize();

            return combined;
        }

        public Vector2 GetAimInput()
        {
            if (rightJoystickInput.sqrMagnitude > 0.01f)
                return rightJoystickInput;

            if (systemAimInput.sqrMagnitude > 0.01f)
                return systemAimInput;

            return Vector2.zero;
        }

        public bool ConsumeDashPressed()
        {
            if (dashPressed)
            {
                dashPressed = false;
                return true;
            }
            return false;
        }
    }
}

#region OLD_SCRIPT
//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using Sirenix.OdinInspector;
//using Joystick = Iztar.InputSystem.Joystick;

//namespace Iztar.Manager
//{
//    public class InputManager : MonoBehaviour
//    {
//        public static InputManager Instance { get; private set; }

//        [Header("Custom Joystick (Mobile)")]
//        [SerializeField] private Joystick leftJoystick;
//        [SerializeField] private Joystick rightJoystick;

//        private ShipInputActions shipInputActions;

//        [ShowInInspector, ReadOnly, BoxGroup("Input Data")]
//        private Vector2 systemMovementInput;

//        [ShowInInspector, ReadOnly, BoxGroup("Input Data")]
//        private Vector2 leftJoystickInput;

//        [ShowInInspector, ReadOnly, BoxGroup("Input Data")]
//        private Vector2 systemAimInput;

//        [ShowInInspector, ReadOnly, BoxGroup("Input Data")]
//        private Vector2 rightJoystickInput;

//        [ShowInInspector, ReadOnly, BoxGroup("Input Data")]
//        private bool dashPressed;

//        // Delegate references to safely unsubscribe
//        private Action<InputAction.CallbackContext> movePerformedHandler;
//        private Action<InputAction.CallbackContext> moveCanceledHandler;
//        private Action<InputAction.CallbackContext> dashPerformedHandler;
//        private Action<InputAction.CallbackContext> aimPerformedHandler;
//        private Action<InputAction.CallbackContext> aimCanceledHandler;

//        private Action<Vector2> leftJoystickHandler;
//        private Action<Vector2> rightJoystickHandler;

//        public event Action<Vector2> OnAimInput;

//        private void Awake()
//        {
//            SetupSingleton();
//            shipInputActions = new ShipInputActions();

//            // Input System handlers
//            movePerformedHandler = ctx => systemMovementInput = ctx.ReadValue<Vector2>();
//            moveCanceledHandler = ctx => systemMovementInput = Vector2.zero;
//            dashPerformedHandler = ctx => dashPressed = true;

//            aimPerformedHandler = ctx =>
//            {
//                systemAimInput = ctx.ReadValue<Vector2>();
//                OnAimInput?.Invoke(systemAimInput);
//            };

//            aimCanceledHandler = ctx =>
//            {
//                systemAimInput = Vector2.zero;
//                OnAimInput?.Invoke(systemAimInput);
//            };

//            // Joystick handlers
//            leftJoystickHandler = val => leftJoystickInput = val;
//            rightJoystickHandler = val =>
//            {
//                rightJoystickInput = val;
//                OnAimInput?.Invoke(val);
//            };
//        }

//        private void OnEnable()
//        {
//            shipInputActions.Enable();

//            // Input System bindings
//            shipInputActions.ShipController.Move.performed += movePerformedHandler;
//            shipInputActions.ShipController.Move.canceled += moveCanceledHandler;
//            shipInputActions.ShipController.Dash.performed += dashPerformedHandler;
//            shipInputActions.ShipController.Aim.performed += aimPerformedHandler;
//            shipInputActions.ShipController.Aim.canceled += aimCanceledHandler;

//            // Joystick bindings
//            if (leftJoystick != null) leftJoystick.OnDirectionChanged += leftJoystickHandler;
//            if (rightJoystick != null) rightJoystick.OnDirectionChanged += rightJoystickHandler;
//        }

//        private void OnDisable()
//        {
//            // Input System unbind
//            shipInputActions.ShipController.Move.performed -= movePerformedHandler;
//            shipInputActions.ShipController.Move.canceled -= moveCanceledHandler;
//            shipInputActions.ShipController.Dash.performed -= dashPerformedHandler;
//            shipInputActions.ShipController.Aim.performed -= aimPerformedHandler;
//            shipInputActions.ShipController.Aim.canceled -= aimCanceledHandler;

//            // Joystick unbind
//            if (leftJoystick != null) leftJoystick.OnDirectionChanged -= leftJoystickHandler;
//            if (rightJoystick != null) rightJoystick.OnDirectionChanged -= rightJoystickHandler;

//            shipInputActions.Disable();
//        }

//        private void SetupSingleton()
//        {
//            if (Instance != null && Instance != this)
//            {
//                Destroy(gameObject);
//                return;
//            }
//            Instance = this;
//        }

//        // Public API
//        public Vector2 GetMoveInput()
//        {
//            Vector2 combined = systemMovementInput;

//            if (leftJoystickInput.sqrMagnitude > 0.01f)
//                combined += leftJoystickInput;

//            if (combined.sqrMagnitude > 1f)
//                combined.Normalize();

//            return combined;
//        }

//        public Vector2 GetAimInput()
//        {
//            if (rightJoystickInput.sqrMagnitude > 0.01f)
//                return rightJoystickInput;

//            if (systemAimInput.sqrMagnitude > 0.01f)
//                return systemAimInput;

//            return Vector2.zero;
//        }

//        public bool ConsumeDashPressed()
//        {
//            if (dashPressed)
//            {
//                dashPressed = false;
//                return true;
//            }
//            return false;
//        }
//    }
//}
#endregion
