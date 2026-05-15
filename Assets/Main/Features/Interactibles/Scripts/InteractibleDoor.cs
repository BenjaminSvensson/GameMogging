using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractibleDoor : MonoBehaviour, IInteractable
{
    [Serializable]
    public class GameObjectEvent : UnityEvent<GameObject>
    {
    }

    [Header("Interaction")]
    [SerializeField] private string openPrompt = "Open Door";
    [SerializeField] private string closePrompt = "Close Door";
    [SerializeField] private string lockedPrompt = "Locked";
    [SerializeField] private bool isLocked;
    [SerializeField] private bool requiresKey;
    [SerializeField] private string requiredKeyId = "Key_A";
    [SerializeField] private bool unlockWhenCorrectKeyUsed = true;

    [Header("Door")]
    [SerializeField] private Transform hinge;
    [SerializeField] private bool startsOpen;
    [SerializeField] private float openAngle = 100f;
    [SerializeField] private bool invertOpenDirection;
    [SerializeField] private float openDuration = 0.35f;
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Events")]
    [SerializeField] private UnityEvent onLockedInteract;
    [SerializeField] private UnityEvent onUnlockedWithKey;
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [SerializeField] private GameObjectEvent onInteractedBy;

    private Quaternion closedLocalRotation;
    private Coroutine moveRoutine;
    private bool isOpen;
    private int openDirection = 1;

    public bool IsLocked => isLocked;
    public bool IsOpen => isOpen;
    public string InteractionPrompt => IsLockedFor(null) ? lockedPrompt : isOpen ? closePrompt : openPrompt;

    private void Awake()
    {
        if (hinge == null)
        {
            hinge = transform;
        }

        closedLocalRotation = hinge.localRotation;

        if (startsOpen)
        {
            isOpen = true;
            hinge.localRotation = GetOpenRotation(openDirection);
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled;
    }

    public void Interact(GameObject interactor)
    {
        onInteractedBy?.Invoke(interactor);

        if (IsLockedFor(interactor))
        {
            onLockedInteract?.Invoke();
            return;
        }

        TryUnlockWithKey(interactor);

        if (isOpen)
        {
            Close();
            return;
        }

        OpenAwayFrom(interactor);
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

    private bool IsLockedFor(GameObject interactor)
    {
        if (!isLocked)
        {
            return false;
        }

        if (!requiresKey)
        {
            return true;
        }

        PlayerKeyInventory inventory = GetInventory(interactor);
        return inventory == null || !inventory.HasKey(requiredKeyId);
    }

    private void TryUnlockWithKey(GameObject interactor)
    {
        if (!isLocked || !requiresKey)
        {
            return;
        }

        PlayerKeyInventory inventory = GetInventory(interactor);
        if (inventory == null || !inventory.HasKey(requiredKeyId))
        {
            return;
        }

        onUnlockedWithKey?.Invoke();

        if (unlockWhenCorrectKeyUsed)
        {
            Unlock();
        }
    }

    private PlayerKeyInventory GetInventory(GameObject interactor)
    {
        return interactor != null ? interactor.GetComponentInParent<PlayerKeyInventory>() : null;
    }

    public void ToggleLocked()
    {
        SetLocked(!isLocked);
    }

    public void OpenAwayFrom(GameObject interactor)
    {
        if (interactor != null)
        {
            openDirection = GetDirectionAwayFrom(interactor.transform.position);
        }

        isOpen = true;
        StartMove(GetOpenRotation(openDirection), onOpened);
    }

    public void Close()
    {
        isOpen = false;
        StartMove(closedLocalRotation, onClosed);
    }

    private int GetDirectionAwayFrom(Vector3 interactorPosition)
    {
        Vector3 toInteractor = interactorPosition - hinge.position;
        float side = Vector3.Dot(hinge.forward, toInteractor);
        int direction = side >= 0f ? 1 : -1;

        return invertOpenDirection ? -direction : direction;
    }

    private Quaternion GetOpenRotation(int direction)
    {
        return closedLocalRotation * Quaternion.Euler(0f, Mathf.Abs(openAngle) * direction, 0f);
    }

    private void StartMove(Quaternion targetRotation, UnityEvent completedEvent)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveToRotation(targetRotation, completedEvent));
    }

    private IEnumerator MoveToRotation(Quaternion targetRotation, UnityEvent completedEvent)
    {
        Quaternion startRotation = hinge.localRotation;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = motionCurve != null ? motionCurve.Evaluate(t) : t;
            hinge.localRotation = Quaternion.Slerp(startRotation, targetRotation, curvedT);
            yield return null;
        }

        hinge.localRotation = targetRotation;
        moveRoutine = null;
        completedEvent?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Transform hingeTransform = hinge != null ? hinge : transform;

        Gizmos.color = new Color(0.15f, 0.65f, 1f, 1f);
        Gizmos.DrawLine(hingeTransform.position, hingeTransform.position + hingeTransform.up * 1.5f);

        Gizmos.color = new Color(0.15f, 0.65f, 1f, 0.35f);
        Gizmos.DrawRay(hingeTransform.position, hingeTransform.forward);
    }
}
