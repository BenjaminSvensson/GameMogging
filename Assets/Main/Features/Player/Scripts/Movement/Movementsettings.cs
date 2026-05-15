using UnityEngine;

public class Movementsettings : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float walkSpeed = 4.2f;
    [SerializeField, Min(0f)] private float runSpeed = 7.5f;
    [SerializeField, Min(0f)] private float groundAcceleration = 48f;
    [SerializeField, Min(0f)] private float groundDeceleration = 36f;
    [SerializeField, Min(0f)] private float airAcceleration = 24f;
    [SerializeField, Min(0f)] private float airControl = 7f;
    [SerializeField, Min(0f)] private float airSpeedCap = 8.5f;
    [SerializeField, Min(0f)] private float maxBunnyHopSpeed = 12.5f;
    [SerializeField, Min(1f)] private float bunnyHopSpeedMultiplier = 1.04f;
    [SerializeField, Min(0f)] private float rotationSpeed = 18f;
    [SerializeField, Range(0f, 1f)] private float inputDeadZone = 0.12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -32f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Jump")]
    [SerializeField, Min(0f)] private float jumpHeight = 1.55f;
    [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
    [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;
    [Tooltip("Holding jump keeps the jump buffer hot, making repeated bunny hops consistent.")]
    [SerializeField] private bool holdJumpToBunnyHop = true;

    [Header("Fixed Camera Feel")]
    [Tooltip("Keeps the current input camera direction while the player holds movement, then updates when input is released.")]
    [SerializeField] private bool lockMovementBasisUntilInputReleased = true;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float GroundAcceleration => groundAcceleration;
    public float GroundDeceleration => groundDeceleration;
    public float AirAcceleration => airAcceleration;
    public float AirControl => airControl;
    public float AirSpeedCap => airSpeedCap;
    public float MaxBunnyHopSpeed => maxBunnyHopSpeed;
    public float BunnyHopSpeedMultiplier => bunnyHopSpeedMultiplier;
    public float RotationSpeed => rotationSpeed;
    public float InputDeadZone => inputDeadZone;
    public float Gravity => gravity;
    public float GroundedStickForce => groundedStickForce;
    public float JumpHeight => jumpHeight;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public bool HoldJumpToBunnyHop => holdJumpToBunnyHop;
    public bool LockMovementBasisUntilInputReleased => lockMovementBasisUntilInputReleased;
}
