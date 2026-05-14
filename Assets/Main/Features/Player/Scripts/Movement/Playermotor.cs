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
    private float verticalVelocity;
    private bool hasLockedBasis;

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

        UpdateMovementBasis(hasMovementInput);
        ApplyGravity();

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

        Vector3 velocity = (moveDirection * settings.WalkSpeed) + (Vector3.up * verticalVelocity);
        characterController.Move(velocity * Time.deltaTime);
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

        basisForward = Vector3.ProjectOnPlane(movementBasis.forward, Vector3.up).normalized;
        basisRight = Vector3.ProjectOnPlane(movementBasis.right, Vector3.up).normalized;

        if (basisForward.sqrMagnitude <= 0.001f)
        {
            basisForward = transform.forward;
        }

        if (basisRight.sqrMagnitude <= 0.001f)
        {
            basisRight = Vector3.Cross(Vector3.up, basisForward).normalized;
        }
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
