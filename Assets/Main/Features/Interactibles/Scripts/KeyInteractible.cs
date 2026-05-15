using System;
using UnityEngine;
using UnityEngine.Events;

public class KeyInteractible : MonoBehaviour, IInteractable
{
    [Serializable]
    public class StringEvent : UnityEvent<string>
    {
    }

    [Header("Key")]
    [SerializeField] private string keyId = "Key_A";
    [SerializeField] private string interactionPrompt = "Pick Up Key";
    [SerializeField] private bool disableAfterPickup = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onPickedUp;
    [SerializeField] private StringEvent onKeyPickedUp;

    private bool isPickedUp;

    public string KeyId => keyId;
    public string InteractionPrompt => interactionPrompt;

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled && !isPickedUp && GetInventory(interactor) != null;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        PlayerKeyInventory inventory = GetInventory(interactor);
        inventory.AddKey(keyId);
        isPickedUp = true;

        onPickedUp?.Invoke();
        onKeyPickedUp?.Invoke(keyId);

        if (disableAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }

    private PlayerKeyInventory GetInventory(GameObject interactor)
    {
        return interactor != null ? interactor.GetComponentInParent<PlayerKeyInventory>() : null;
    }
}
