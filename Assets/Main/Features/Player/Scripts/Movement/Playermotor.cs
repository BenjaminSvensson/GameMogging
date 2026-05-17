using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputReader))]
public class Playermotor : MonoBehaviour
{
    [SerializeField] private Movementsettings settings;

    private CharacterController characterController;
    private PlayerInputReader inputReader;
    private Transform movementBasis;
    private Vector3 basisForward;
    private Vector3 basisRight;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool hasLockedBasis;
    private bool wasGrounded;

    public event Action Jumped;
    public event Action Landed;

    public bool IsGrounded => characterController != null && characterController.isGrounded;
    public bool IsRunning => inputReader != null && inputReader.IsRunHeld;
    public float HorizontalSpeed => horizontalVelocity.magnitude;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputReader = GetComponent<PlayerInputReader>();

        if (settings == null)
        {
            settings = GetComponent<Movementsettings>();
        }
    }

    private void Update()
    {
        if (settings == null)
        {
            return;
        }

        Vector2 moveInput = Vector2.ClampMagnitude(inputReader.MoveInput, 1f);
        bool hasMovementInput = moveInput.sqrMagnitude > settings.InputDeadZone * settings.InputDeadZone;
        wasGrounded = characterController.isGrounded;

        UpdateMovementBasis(hasMovementInput);
        UpdateJumpTimers();

        Vector3 moveDirection = GetMoveDirection(moveInput, hasMovementInput);
        float movementSpeed = inputReader.IsRunHeld ? settings.RunSpeed : settings.WalkSpeed;
        UpdateHorizontalVelocity(moveDirection, movementSpeed, hasMovementInput);

        TryJump();
        ApplyGravity();

        Vector3 velocity = horizontalVelocity + (Vector3.up * verticalVelocity);
        CollisionFlags collisionFlags = characterController.Move(velocity * Time.deltaTime);

        if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        if (!wasGrounded && characterController.isGrounded)
        {
            Landed?.Invoke();
        }
    }

    private Vector3 GetMoveDirection(Vector2 moveInput, bool hasMovementInput)
    {
        Vector3 moveDirection = Vector3.zero;

        if (hasMovementInput)
        {
            moveDirection = (basisRight * moveInput.x) + (basisForward * moveInput.y);
            moveDirection = ApplyTriggerMovementLocks(moveDirection);

            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                RotateTowards(moveDirection);
            }
        }

        return moveDirection;
    }

    private void UpdateHorizontalVelocity(Vector3 moveDirection, float movementSpeed, bool hasMovementInput)
    {
        if (characterController.isGrounded)
        {
            if (hasMovementInput)
            {
                Vector3 targetVelocity = moveDirection * movementSpeed;

                if (jumpBufferTimer <= 0f || horizontalVelocity.magnitude <= movementSpeed)
                {
                    horizontalVelocity = Vector3.MoveTowards(
                        horizontalVelocity,
                        targetVelocity,
                        settings.GroundAcceleration * Time.deltaTime);
                }
            }
            else if (jumpBufferTimer <= 0f)
            {
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    settings.GroundDeceleration * Time.deltaTime);
            }
        }
        else if (hasMovementInput)
        {
            AirAccelerate(moveDirection, movementSpeed);
            ApplyAirControl(moveDirection);
        }

        ClampHorizontalSpeed();
        horizontalVelocity = ApplyTriggerMovementLocks(horizontalVelocity);
    }

    private void AirAccelerate(Vector3 moveDirection, float movementSpeed)
    {
        float wishSpeed = Mathf.Min(movementSpeed, settings.AirSpeedCap);
        float currentSpeed = Vector3.Dot(horizontalVelocity, moveDirection);
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0f)
        {
            return;
        }

        float acceleration = settings.AirAcceleration * movementSpeed * Time.deltaTime;
        horizontalVelocity += moveDirection * Mathf.Min(acceleration, addSpeed);
    }

    private void ApplyAirControl(Vector3 moveDirection)
    {
        float speed = horizontalVelocity.magnitude;

        if (speed <= 0.001f || Vector3.Dot(horizontalVelocity.normalized, moveDirection) <= 0f)
        {
            return;
        }

        Vector3 controlledDirection = Vector3.Slerp(
            horizontalVelocity.normalized,
            moveDirection,
            settings.AirControl * Time.deltaTime).normalized;

        horizontalVelocity = controlledDirection * speed;
    }

    private void UpdateMovementBasis(bool hasMovementInput)
    {
        if (!settings.LockMovementBasisUntilInputReleased || !hasMovementInput || !hasLockedBasis)
        {
            movementBasis = FixedPerspectiveCameraController.ActiveMovementReference;

            if (movementBasis == null && UnityEngine.Camera.main != null)
            {
                movementBasis = UnityEngine.Camera.main.transform;
            }

            CalculateBasisVectors();
            hasLockedBasis = hasMovementInput;
        }

        if (!hasMovementInput)
        {
            hasLockedBasis = false;
        }
    }

    private void CalculateBasisVectors()
    {
        if (movementBasis == null)
        {
            basisForward = transform.forward;
            basisRight = transform.right;
            return;
        }

        basisRight = Vector3.ProjectOnPlane(movementBasis.right, Vector3.up).normalized;

        if (basisRight.sqrMagnitude > 0.001f)
        {
            basisForward = Vector3.Cross(basisRight, Vector3.up).normalized;
            return;
        }

        basisForward = Vector3.ProjectOnPlane(movementBasis.forward, Vector3.up).normalized;

        if (basisForward.sqrMagnitude > 0.001f)
        {
            basisRight = Vector3.Cross(Vector3.up, basisForward).normalized;
            return;
        }

        basisForward = transform.forward;
        basisRight = transform.right;
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = settings.GroundedStickForce;
            return;
        }

        verticalVelocity += settings.Gravity * Time.deltaTime;
    }

    private void UpdateJumpTimers()
    {
        if (characterController.isGrounded)
        {
            coyoteTimer = settings.CoyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (inputReader.ConsumeJumpPressed())
        {
            jumpBufferTimer = settings.JumpBufferTime;
        }
        else if (settings.HoldJumpToBunnyHop && inputReader.IsJumpHeld && coyoteTimer > 0f)
        {
            jumpBufferTimer = settings.JumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }
    }

    private void TryJump()
    {
        if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
        {
            return;
        }

        verticalVelocity = Mathf.Sqrt(settings.JumpHeight * -2f * settings.Gravity);
        BoostBunnyHopSpeed();
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        Jumped?.Invoke();
    }

    private void BoostBunnyHopSpeed()
    {
        float speed = horizontalVelocity.magnitude;

        if (speed <= settings.RunSpeed * 0.9f)
        {
            return;
        }

        horizontalVelocity *= settings.BunnyHopSpeedMultiplier;
        ClampHorizontalSpeed();
    }

    private void ClampHorizontalSpeed()
    {
        float speed = horizontalVelocity.magnitude;

        if (speed > settings.MaxBunnyHopSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * settings.MaxBunnyHopSpeed;
        }
    }

    private Vector3 ApplyTriggerMovementLocks(Vector3 moveDirection)
    {
        CameraAreaTrigger activeZone = FixedPerspectiveCameraController.ActiveZone;
        if (activeZone == null)
        {
            return moveDirection;
        }

        return Vector3.Scale(moveDirection, activeZone.PlayerMovementAxisMask);
    }

    private void RotateTowards(Vector3 moveDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, settings.RotationSpeed * Time.deltaTime);
    }
}
