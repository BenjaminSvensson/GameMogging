using System;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour, IInteractable
{
    [Serializable]
    public class GameObjectEvent : UnityEvent<GameObject>
    {
    }

    [SerializeField] private string interactionPrompt = "Interact";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool disableAfterInteraction;
    [SerializeField] private UnityEvent onInteracted;
    [SerializeField] private GameObjectEvent onInteractedBy;

    public string InteractionPrompt => interactionPrompt;

    public bool CanInteract(GameObject interactor)
    {
        return canInteract && isActiveAndEnabled;
    }

    public void SetCanInteract(bool value)
    {
        canInteract = value;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        onInteracted?.Invoke();
        onInteractedBy?.Invoke(interactor);

        if (disableAfterInteraction)
        {
            canInteract = false;
        }
    }
}
