using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }

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
}
