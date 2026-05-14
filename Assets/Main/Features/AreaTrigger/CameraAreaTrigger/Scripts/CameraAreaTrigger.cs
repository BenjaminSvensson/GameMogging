using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraAreaTrigger : MonoBehaviour
{
    public enum CameraMode
    {
        FixedPosition,
        FollowPlayer
    }

    [Header("Activation")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int priority;

    [Header("Camera Pose")]
    [SerializeField] private CameraMode mode = CameraMode.FixedPosition;
    [SerializeField] private Transform cameraPose;
    [SerializeField] private Vector3 cameraPosition;
    [SerializeField] private Vector3 cameraEulerAngles;

    [Header("Transition")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField, Min(0.01f)] private float transitionDuration = 0.55f;
    [SerializeField, Min(0f)] private float followSmoothing = 8f;

    [Header("Follow Player")]
    [SerializeField] private bool followWorldX = true;
    [SerializeField] private bool followWorldY;
    [SerializeField] private bool followWorldZ = true;
    [SerializeField] private Vector3 followScale = Vector3.one;

    public int Priority => priority;
    public CameraMode Mode => mode;
    public bool SmoothTransition => smoothTransition;
    public float TransitionDuration => transitionDuration;
    public float FollowSmoothing => followSmoothing;
    public Vector3 FollowAxisMask => new Vector3(followWorldX ? 1f : 0f, followWorldY ? 1f : 0f, followWorldZ ? 1f : 0f);
    public Vector3 FollowScale => followScale;

    public Vector3 CameraPosition => cameraPose != null ? cameraPose.position : cameraPosition;
    public Quaternion CameraRotation => cameraPose != null ? cameraPose.rotation : Quaternion.Euler(cameraEulerAngles);

    private void Reset()
    {
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        cameraPosition = transform.position + new Vector3(0f, 3f, -6f);
        cameraEulerAngles = new Vector3(20f, 0f, 0f);
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

        cameraPose = null;
        cameraPosition = UnityEngine.Camera.main.transform.position;
        cameraEulerAngles = UnityEngine.Camera.main.transform.eulerAngles;
    }

    [ContextMenu("Capture Camera Pose From This Transform")]
    private void CaptureFromThisTransform()
    {
        cameraPose = null;
        cameraPosition = transform.position;
        cameraEulerAngles = transform.eulerAngles;
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
        Gizmos.DrawLine(transform.position, CameraPosition);
        Gizmos.DrawWireSphere(CameraPosition, 0.25f);
    }
}
