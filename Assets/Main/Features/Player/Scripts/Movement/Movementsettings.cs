using UnityEngine;

public class Movementsettings : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float walkSpeed = 3.2f;
    [SerializeField, Min(0f)] private float runSpeed = 5.4f;
    [SerializeField, Min(0f)] private float rotationSpeed = 12f;
    [SerializeField, Range(0f, 1f)] private float inputDeadZone = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpHeight = 1.35f;
    [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
    [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;

    [Header("Fixed Camera Feel")]
    [Tooltip("Keeps the current input camera direction while the player holds movement, then updates when input is released.")]
    [SerializeField] private bool lockMovementBasisUntilInputReleased = true;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float RotationSpeed => rotationSpeed;
    public float InputDeadZone => inputDeadZone;
    public float Gravity => gravity;
    public float GroundedStickForce => groundedStickForce;
    public float JumpHeight => jumpHeight;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public bool LockMovementBasisUntilInputReleased => lockMovementBasisUntilInputReleased;
}
