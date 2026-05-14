using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions inputActions;
    private bool interactPressedQueued;

    public Vector2 MoveInput { get; private set; }
    public bool IsRunHeld { get; private set; }
    public event Action InteractPressed;

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
}
