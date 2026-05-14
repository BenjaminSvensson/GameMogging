using System;
using UnityEngine;

[RequireComponent(typeof(PlayerInputReader))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private float interactionRange = 1.8f;
    [SerializeField] private float interactionRadius = 0.6f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    [Header("Line Of Sight")]
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private LayerMask obstructionLayers = ~0;

    private const int MaxInteractables = 16;

    private readonly Collider[] overlapResults = new Collider[MaxInteractables];
    private PlayerInputReader inputReader;
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject;

    public event Action<IInteractable> FocusChanged;
    public event Action<IInteractable> Interacted;

    public IInteractable CurrentInteractable => currentInteractable;
    public string CurrentPrompt => currentInteractable?.InteractionPrompt ?? string.Empty;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();

        if (interactionOrigin == null)
        {
            interactionOrigin = transform;
        }
    }

    private void Update()
    {
        SetCurrentInteractable(FindBestInteractable());

        if (inputReader.ConsumeInteractPressed())
        {
            TryInteract();
        }
    }

    public bool TryInteract()
    {
        if (currentInteractable == null || currentInteractableObject == null)
        {
            return false;
        }

        if (!currentInteractable.CanInteract(gameObject))
        {
            SetCurrentInteractable(null);
            return false;
        }

        IInteractable interacted = currentInteractable;
        interacted.Interact(gameObject);
        Interacted?.Invoke(interacted);
        SetCurrentInteractable(FindBestInteractable());
        return true;
    }

    private IInteractable FindBestInteractable()
    {
        Vector3 origin = interactionOrigin.position;
        Vector3 center = origin + interactionOrigin.forward * interactionRange;
        int hitCount = Physics.OverlapSphereNonAlloc(center, interactionRadius, overlapResults, interactableLayers, triggerInteraction);

        IInteractable bestInteractable = null;
        GameObject bestGameObject = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider candidateCollider = overlapResults[i];
            if (candidateCollider == null)
            {
                continue;
            }

            IInteractable candidate = candidateCollider.GetComponentInParent<IInteractable>();
            if (candidate == null || !candidate.CanInteract(gameObject))
            {
                continue;
            }

            GameObject candidateGameObject = GetInteractableGameObject(candidate);
            if (candidateGameObject == null || !HasLineOfSight(origin, candidateCollider))
            {
                continue;
            }

            float score = GetInteractionScore(origin, candidateCollider);
            if (score < bestScore)
            {
                bestScore = score;
                bestInteractable = candidate;
                bestGameObject = candidateGameObject;
            }
        }

        currentInteractableObject = bestGameObject;
        return bestInteractable;
    }

    private float GetInteractionScore(Vector3 origin, Collider candidateCollider)
    {
        Vector3 closestPoint = candidateCollider.ClosestPoint(origin);
        Vector3 toCandidate = closestPoint - origin;
        float distanceScore = toCandidate.sqrMagnitude;
        float facingScore = 1f - Mathf.Clamp01(Vector3.Dot(interactionOrigin.forward, toCandidate.normalized));

        return distanceScore + facingScore;
    }

    private bool HasLineOfSight(Vector3 origin, Collider candidateCollider)
    {
        if (!requireLineOfSight)
        {
            return true;
        }

        Vector3 target = candidateCollider.bounds.center;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        if (!Physics.Raycast(origin, direction / distance, out RaycastHit hit, distance, obstructionLayers, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.collider == candidateCollider || hit.collider.transform.IsChildOf(candidateCollider.transform);
    }

    private GameObject GetInteractableGameObject(IInteractable interactable)
    {
        return interactable is Component component ? component.gameObject : null;
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (ReferenceEquals(currentInteractable, interactable))
        {
            return;
        }

        currentInteractable = interactable;
        FocusChanged?.Invoke(currentInteractable);
    }

    private void OnDrawGizmosSelected()
    {
        Transform originTransform = interactionOrigin != null ? interactionOrigin : transform;
        Vector3 center = originTransform.position + originTransform.forward * interactionRange;

        Gizmos.color = new Color(0.1f, 0.8f, 0.45f, 0.35f);
        Gizmos.DrawSphere(center, interactionRadius);
        Gizmos.color = new Color(0.1f, 0.8f, 0.45f, 1f);
        Gizmos.DrawLine(originTransform.position, center);
    }
}
