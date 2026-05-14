using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraAreaTrigger : MonoBehaviour
{
    public enum CameraMode
    {
        FixedPosition,
        FollowPlayer
    }

    public enum FollowStyle
    {
        MoveCamera,
        RotateCamera
    }

    [Header("Activation")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int priority;

    [Header("Camera Pose")]
    [SerializeField] private CameraMode mode = CameraMode.FixedPosition;
    [Tooltip("Move and rotate this transform to set the exact camera view for this trigger.")]
    [SerializeField] private Transform cameraPose;
    [HideInInspector]
    [SerializeField] private Vector3 cameraPosition;
    [HideInInspector]
    [SerializeField] private Vector3 cameraEulerAngles;

    [Header("Transition")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.55f;
    [SerializeField, Min(0f)] private float followSmoothing = 8f;

    [Header("Follow Player")]
    [SerializeField] private FollowStyle followStyle = FollowStyle.MoveCamera;
    [SerializeField] private bool followWorldX = true;
    [SerializeField] private bool followWorldY;
    [SerializeField] private bool followWorldZ = true;
    [SerializeField] private Vector3 followScale = Vector3.one;
    [SerializeField] private Vector3 rotationTargetOffset = new Vector3(0f, 1f, 0f);

    [Header("Player Movement Lock")]
    [SerializeField] private bool lockPlayerWorldX;
    [SerializeField] private bool lockPlayerWorldZ;

    public int Priority => priority;
    public CameraMode Mode => mode;
    public bool SmoothTransition => smoothTransition;
    public float TransitionDuration => transitionDuration;
    public float FollowSmoothing => followSmoothing;
    public FollowStyle PlayerFollowStyle => followStyle;
    public Vector3 FollowAxisMask => new Vector3(followWorldX ? 1f : 0f, followWorldY ? 1f : 0f, followWorldZ ? 1f : 0f);
    public Vector3 FollowScale => followScale;
    public Vector3 RotationTargetOffset => rotationTargetOffset;
    public Vector3 PlayerMovementAxisMask => new Vector3(lockPlayerWorldX ? 0f : 1f, 1f, lockPlayerWorldZ ? 0f : 1f);

    public Vector3 CameraPosition => cameraPose != null ? cameraPose.position : cameraPosition;
    public Quaternion CameraRotation => cameraPose != null ? cameraPose.rotation : Quaternion.Euler(cameraEulerAngles);
    public Vector3 FollowOrigin => transform.TransformPoint(GetColliderCenter());

    private void Reset()
    {
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        cameraPosition = transform.position + new Vector3(0f, 3f, -6f);
        cameraEulerAngles = new Vector3(20f, 0f, 0f);
        EnsureCameraPose();
    }

    private void OnValidate()
    {
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        transitionDuration = Mathf.Max(0.01f, transitionDuration);
        followSmoothing = Mathf.Max(0f, followSmoothing);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        FixedPerspectiveCameraController.ActivateZone(this, other.transform);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        FixedPerspectiveCameraController.ActivateZone(this, other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        FixedPerspectiveCameraController.DeactivateZone(this);
    }

    [ContextMenu("Capture Camera Pose From Main Camera")]
    private void CaptureFromMainCamera()
    {
        if (UnityEngine.Camera.main == null)
        {
            return;
        }

        EnsureCameraPose();
        cameraPose.SetPositionAndRotation(UnityEngine.Camera.main.transform.position, UnityEngine.Camera.main.transform.rotation);
    }

    [ContextMenu("Capture Camera Pose From This Transform")]
    private void CaptureFromThisTransform()
    {
        EnsureCameraPose();
        cameraPose.SetPositionAndRotation(transform.position, transform.rotation);
    }

    [ContextMenu("Create Or Repair Camera Pose Empty")]
    private void EnsureCameraPose()
    {
        if (cameraPose != null)
        {
            return;
        }

        Transform existingPose = transform.Find("Camera Pose");
        if (existingPose != null)
        {
            cameraPose = existingPose;
            return;
        }

        GameObject poseObject = new GameObject("Camera Pose");
        cameraPose = poseObject.transform;
        cameraPose.SetParent(transform);
        cameraPose.position = cameraPosition;
        cameraPose.rotation = Quaternion.Euler(cameraEulerAngles);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = mode == CameraMode.FollowPlayer ? new Color(0.1f, 0.7f, 1f, 0.35f) : new Color(1f, 0.75f, 0.1f, 0.35f);
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Vector3 center = boxCollider != null ? boxCollider.center : Vector3.zero;
        Vector3 size = boxCollider != null ? boxCollider.size : Vector3.one;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(center, size);

        Gizmos.color = mode == CameraMode.FollowPlayer ? new Color(0.1f, 0.7f, 1f, 1f) : new Color(1f, 0.75f, 0.1f, 1f);
        Gizmos.DrawWireCube(center, size);

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawLine(FollowOrigin, CameraPosition);
        Gizmos.DrawWireSphere(CameraPosition, 0.25f);
        DrawCameraPoseArrow();
    }

    private Vector3 GetColliderCenter()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        return boxCollider != null ? boxCollider.center : Vector3.zero;
    }

    private void DrawCameraPoseArrow()
    {
        Vector3 arrowStart = CameraPosition;
        Vector3 arrowForward = CameraRotation * Vector3.forward;

        if (arrowForward.sqrMagnitude <= 0.001f)
        {
            return;
        }

        arrowForward.Normalize();

        float arrowLength = 1.35f;
        float arrowHeadLength = 0.35f;
        float arrowHeadAngle = 25f;
        Vector3 arrowEnd = arrowStart + arrowForward * arrowLength;
        Vector3 rightHead = Quaternion.LookRotation(arrowForward) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;
        Vector3 leftHead = Quaternion.LookRotation(arrowForward) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;
        Vector3 upHead = Quaternion.LookRotation(arrowForward) * Quaternion.Euler(180f - arrowHeadAngle, 0f, 0f) * Vector3.forward;
        Vector3 downHead = Quaternion.LookRotation(arrowForward) * Quaternion.Euler(180f + arrowHeadAngle, 0f, 0f) * Vector3.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd + rightHead * arrowHeadLength);
        Gizmos.DrawLine(arrowEnd, arrowEnd + leftHead * arrowHeadLength);
        Gizmos.DrawLine(arrowEnd, arrowEnd + upHead * arrowHeadLength);
        Gizmos.DrawLine(arrowEnd, arrowEnd + downHead * arrowHeadLength);
    }
}
