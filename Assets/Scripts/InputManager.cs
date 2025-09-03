using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Awake()
    {
        SetupSingleton();
        shipInputActions = new ShipInputActions();
    }

    private void OnEnable()
    {
        shipInputActions.Enable();

        // Movement
        shipInputActions.ShipController.Move.performed += ctx => systemMovementInput = ctx.ReadValue<Vector2>();
        shipInputActions.ShipController.Move.canceled += ctx => systemMovementInput = Vector2.zero;

        // Dash
        shipInputActions.ShipController.Dash.performed += ctx => dashPressed = true;

        // Aim
        shipInputActions.ShipController.Aim.performed += ctx => systemAimInput = ctx.ReadValue<Vector2>();
        shipInputActions.ShipController.Aim.canceled += ctx => systemAimInput = Vector2.zero;
    }

    private void OnDisable()
    {
        shipInputActions.ShipController.Move.performed -= ctx => systemMovementInput = ctx.ReadValue<Vector2>();
        shipInputActions.ShipController.Move.canceled -= ctx => systemMovementInput = Vector2.zero;

        shipInputActions.ShipController.Dash.performed -= ctx => dashPressed = true;

        shipInputActions.ShipController.Aim.performed -= ctx => systemAimInput = ctx.ReadValue<Vector2>();
        shipInputActions.ShipController.Aim.canceled -= ctx => systemAimInput = Vector2.zero;

        shipInputActions.Disable();
    }

    private void Update()
    {
        if (leftJoystick != null)
            leftJoystickInput = leftJoystick.Direction;

        if (rightJoystick != null)
            rightJoystickInput = rightJoystick.Direction;
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
        if (rightJoystick != null && rightJoystick.Direction.sqrMagnitude > 0.01f)
            return rightJoystick.Direction;

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
