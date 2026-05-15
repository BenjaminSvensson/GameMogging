using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions inputActions;
    private bool interactPressedQueued;
    private bool jumpPressedQueued;

    public Vector2 MoveInput { get; private set; }
    public bool IsRunHeld { get; private set; }
    public event Action InteractPressed;
    public event Action JumpPressed;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Player.RemoveCallbacks(this);
        inputActions.Dispose();
    }

    private void Update()
    {
        bool keyboardJumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool gamepadJumpPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (!keyboardJumpPressed && !gamepadJumpPressed)
        {
            return;
        }

        jumpPressedQueued = true;
        JumpPressed?.Invoke();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        interactPressedQueued = true;
        InteractPressed?.Invoke();
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        IsRunHeld = context.ReadValueAsButton();
    }

    public bool ConsumeInteractPressed()
    {
        if (!interactPressedQueued)
        {
            return false;
        }

        interactPressedQueued = false;
        return true;
    }

    public bool ConsumeJumpPressed()
    {
        if (!jumpPressedQueued)
        {
            return false;
        }

        jumpPressedQueued = false;
        return true;
    }
}
