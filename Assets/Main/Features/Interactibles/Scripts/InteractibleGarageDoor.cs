using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractibleGarageDoor : MonoBehaviour, IInteractable
{
    [Serializable]
    public class GameObjectEvent : UnityEvent<GameObject>
    {
    }

    [Header("Interaction")]
    [SerializeField] private string openPrompt = "Open Garage Door";
    [SerializeField] private string closePrompt = "Close Garage Door";
    [SerializeField] private string lockedPrompt = "Locked";
    [SerializeField] private bool isLocked;

    [Header("Door")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private bool startsOpen;
    [SerializeField] private float openHeight = 3f;
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Events")]
    [SerializeField] private UnityEvent onLockedInteract;
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [SerializeField] private GameObjectEvent onInteractedBy;

    private Vector3 closedLocalPosition;
    private Coroutine moveRoutine;
    private bool isOpen;

    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;
    public string InteractionPrompt => isLocked ? lockedPrompt : isOpen ? closePrompt : openPrompt;

    private void Awake()
    {
        if (doorPanel == null)
        {
            doorPanel = transform;
        }

        closedLocalPosition = doorPanel.localPosition;

        if (startsOpen)
        {
            isOpen = true;
            doorPanel.localPosition = GetOpenPosition();
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled;
    }

    public void Interact(GameObject interactor)
    {
        onInteractedBy?.Invoke(interactor);

        if (isLocked)
        {
            onLockedInteract?.Invoke();
            return;
        }

        if (isOpen)
        {
            Close();
            return;
        }

        Open();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }

    public void Lock()
    {
        SetLocked(true);
    }

    public void Unlock()
    {
        SetLocked(false);
    }

    public void ToggleLocked()
    {
        SetLocked(!isLocked);
    }

    public void Open()
    {
        isOpen = true;
        StartMove(GetOpenPosition(), onOpened);
    }

    public void Close()
    {
        isOpen = false;
        StartMove(closedLocalPosition, onClosed);
    }

    private Vector3 GetOpenPosition()
    {
        return closedLocalPosition + Vector3.up * Mathf.Abs(openHeight);
    }

    private void StartMove(Vector3 targetPosition, UnityEvent completedEvent)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveToPosition(targetPosition, completedEvent));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, UnityEvent completedEvent)
    {
        Vector3 startPosition = doorPanel.localPosition;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = motionCurve != null ? motionCurve.Evaluate(t) : t;
            doorPanel.localPosition = Vector3.Lerp(startPosition, targetPosition, curvedT);
            yield return null;
        }

        doorPanel.localPosition = targetPosition;
        moveRoutine = null;
        completedEvent?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Transform panel = doorPanel != null ? doorPanel : transform;
        Vector3 closedPosition = Application.isPlaying ? closedLocalPosition : panel.localPosition;
        Vector3 worldClosed = panel.parent != null ? panel.parent.TransformPoint(closedPosition) : closedPosition;
        Vector3 worldOpen = panel.parent != null ? panel.parent.TransformPoint(closedPosition + Vector3.up * Mathf.Abs(openHeight)) : closedPosition + Vector3.up * Mathf.Abs(openHeight);

        Gizmos.color = new Color(1f, 0.7f, 0.15f, 1f);
        Gizmos.DrawLine(worldClosed, worldOpen);
        Gizmos.DrawWireCube(worldOpen, panel.lossyScale);
    }
}
