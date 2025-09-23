using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

using Joystick = Iztar.InputSystem.Joystick;

namespace Iztar.Manager
{
    public class GameplayInputSystem : MonoBehaviour
    {
        public static GameplayInputSystem Instance { get; private set; }

        [Header("Custom Joystick (Mobile)")]
        [SerializeField] private Joystick leftJoystick;
        [SerializeField] private Joystick rightJoystick;

        [Header("Mobile Buttons")]
        [SerializeField] private Button dashButton;

        private ShipInputActions shipInputActions;

        // Input cache
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 systemMovementInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 leftJoystickInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 systemAimInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private Vector2 rightJoystickInput;
        [ShowInInspector, ReadOnly, BoxGroup("Input Data")] private bool dashPressed;
        private bool dashHeldMobile;

        // Delegates
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

            // Joysticks
            leftJoystickHandler = val => leftJoystickInput = val;
            rightJoystickHandler = val =>
            {
                rightJoystickInput = val;
                if (val.sqrMagnitude > 0.01f)
                    OnAimInputPerformed?.Invoke(val);
                else
                    OnAimInputCancelled?.Invoke();
            };

            // Auto setup EventTrigger untuk dashButton
            if (dashButton != null)
            {
                var trigger = dashButton.GetComponent<EventTrigger>();
                if (trigger == null) trigger = dashButton.gameObject.AddComponent<EventTrigger>();
                trigger.triggers ??= new System.Collections.Generic.List<EventTrigger.Entry>();
                trigger.triggers.Clear();

                // Down
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener(_ => dashHeldMobile = true);
                trigger.triggers.Add(down);

                // Up
                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener(_ => dashHeldMobile = false);
                trigger.triggers.Add(up);
            }
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

            if (dashButton != null)
                dashButton.onClick.AddListener(OnDashButtonPressed);
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

            if (dashButton != null)
                dashButton.onClick.RemoveListener(OnDashButtonPressed);

            shipInputActions.Disable();
        }

        private void OnDashButtonPressed()
        {
            // klik sekali = one-shot trigger
            dashPressed = true;
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

        // ---------------- PUBLIC API ----------------

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

        /// OneShot (sekali klik)
        public bool ConsumeDashPressed()
        {
            if (dashPressed)
            {
                dashPressed = false;
                return true;
            }
            return false;
        }

        /// Draining (tahan/lepas)
        public bool IsDashHeld()
        {
            return shipInputActions.ShipController.Dash.IsPressed() || dashHeldMobile;
        }

        public bool IsDashReleased()
        {
            // released dari keyboard OR mobile
            return shipInputActions.ShipController.Dash.WasReleasedThisFrame() || !dashHeldMobile;
        }
    }
}
