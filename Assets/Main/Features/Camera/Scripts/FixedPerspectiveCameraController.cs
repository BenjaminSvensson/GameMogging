using System.Collections.Generic;
using UnityEngine;

public class FixedPerspectiveCameraController : MonoBehaviour
{
    private static readonly List<CameraAreaTrigger> activeZones = new List<CameraAreaTrigger>();
    private static FixedPerspectiveCameraController instance;

    [Header("References")]
    [SerializeField] private UnityEngine.Camera cameraToControl;
    [SerializeField] private Transform player;
    [SerializeField] private Transform fallbackMovementReference;

    [Header("Default Camera")]
    [SerializeField] private Transform defaultCameraPose;
    [SerializeField] private bool snapToDefaultOnStart = true;

    private CameraAreaTrigger currentZone;
    private Transform currentPlayer;
    private Vector3 followEntryPlayerPosition;
    private Vector3 followEntryCameraPosition;
    private Vector3 transitionStartPosition;
    private Quaternion transitionStartRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float transitionElapsed;

    public static Transform ActiveMovementReference
    {
        get
        {
            if (instance == null)
            {
                return null;
            }

            return instance.cameraToControl != null ? instance.cameraToControl.transform : instance.fallbackMovementReference;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("There is more than one FixedPerspectiveCameraController in the scene. The newest one will be used.", this);
        }

        instance = this;

        if (cameraToControl == null)
        {
            cameraToControl = GetComponent<UnityEngine.Camera>();
        }

        if (cameraToControl == null)
        {
            cameraToControl = UnityEngine.Camera.main;
        }
    }

    private void Start()
    {
        if (cameraToControl == null)
        {
            enabled = false;
            return;
        }

        if (snapToDefaultOnStart && defaultCameraPose != null)
        {
            cameraToControl.transform.SetPositionAndRotation(defaultCameraPose.position, defaultCameraPose.rotation);
        }

        targetPosition = cameraToControl.transform.position;
        targetRotation = cameraToControl.transform.rotation;
    }

    private void LateUpdate()
    {
        if (cameraToControl == null)
        {
            return;
        }

        CameraAreaTrigger bestZone = GetBestActiveZone();
        if (bestZone != currentZone)
        {
            SetCurrentZone(bestZone);
        }

        UpdateTargetPose();
        MoveCamera();
    }

    public static void ActivateZone(CameraAreaTrigger zone, Transform zonePlayer)
    {
        if (zone == null)
        {
            return;
        }

        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
        }

        if (instance != null)
        {
            instance.currentPlayer = zonePlayer;
        }
    }

    public static void DeactivateZone(CameraAreaTrigger zone)
    {
        if (zone == null)
        {
            return;
        }

        activeZones.Remove(zone);
    }

    private CameraAreaTrigger GetBestActiveZone()
    {
        CameraAreaTrigger bestZone = null;

        for (int i = activeZones.Count - 1; i >= 0; i--)
        {
            CameraAreaTrigger zone = activeZones[i];
            if (zone == null || !zone.isActiveAndEnabled)
            {
                activeZones.RemoveAt(i);
                continue;
            }

            if (bestZone == null || zone.Priority >= bestZone.Priority)
            {
                bestZone = zone;
            }
        }

        return bestZone;
    }

    private void SetCurrentZone(CameraAreaTrigger zone)
    {
        currentZone = zone;
        transitionElapsed = 0f;
        transitionStartPosition = cameraToControl.transform.position;
        transitionStartRotation = cameraToControl.transform.rotation;

        if (player != null)
        {
            currentPlayer = player;
        }

        if (currentZone != null)
        {
            followEntryPlayerPosition = currentPlayer != null ? currentPlayer.position : Vector3.zero;
            followEntryCameraPosition = currentZone.CameraPosition;

            if (!currentZone.SmoothTransition)
            {
                UpdateTargetPose();
                cameraToControl.transform.SetPositionAndRotation(targetPosition, targetRotation);
            }
        }
        else if (defaultCameraPose != null)
        {
            targetPosition = defaultCameraPose.position;
            targetRotation = defaultCameraPose.rotation;
        }
    }

    private void UpdateTargetPose()
    {
        if (currentZone == null)
        {
            if (defaultCameraPose != null)
            {
                targetPosition = defaultCameraPose.position;
                targetRotation = defaultCameraPose.rotation;
            }

            return;
        }

        targetPosition = currentZone.CameraPosition;
        targetRotation = currentZone.CameraRotation;

        if (currentZone.Mode != CameraAreaTrigger.CameraMode.FollowPlayer || currentPlayer == null)
        {
            return;
        }

        Vector3 playerDelta = currentPlayer.position - followEntryPlayerPosition;

        playerDelta = Vector3.Scale(playerDelta, currentZone.FollowAxisMask);
        playerDelta = Vector3.Scale(playerDelta, currentZone.FollowScale);
        targetPosition = followEntryCameraPosition + playerDelta;
    }

    private void MoveCamera()
    {
        if (currentZone != null && !currentZone.SmoothTransition)
        {
            cameraToControl.transform.SetPositionAndRotation(targetPosition, targetRotation);
            return;
        }

        float duration = currentZone != null ? currentZone.TransitionDuration : 0.55f;
        transitionElapsed += Time.deltaTime;
        float transitionT = Mathf.Clamp01(transitionElapsed / duration);
        transitionT = transitionT * transitionT * (3f - (2f * transitionT));

        Vector3 smoothedPosition = Vector3.Lerp(transitionStartPosition, targetPosition, transitionT);
        Quaternion smoothedRotation = Quaternion.Slerp(transitionStartRotation, targetRotation, transitionT);

        if (currentZone != null && currentZone.Mode == CameraAreaTrigger.CameraMode.FollowPlayer && transitionT >= 1f)
        {
            float followAmount = currentZone.FollowSmoothing <= 0f ? 1f : 1f - Mathf.Exp(-currentZone.FollowSmoothing * Time.deltaTime);
            smoothedPosition = Vector3.Lerp(cameraToControl.transform.position, targetPosition, followAmount);
            smoothedRotation = Quaternion.Slerp(cameraToControl.transform.rotation, targetRotation, followAmount);
        }

        cameraToControl.transform.SetPositionAndRotation(smoothedPosition, smoothedRotation);
    }
}
