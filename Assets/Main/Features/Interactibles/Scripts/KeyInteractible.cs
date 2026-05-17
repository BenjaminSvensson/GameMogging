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

    [Header("Audio")]
    [SerializeField] private AudioClip[] pickupClips;
    [SerializeField, Range(0f, 1f)] private float pickupVolume = 0.9f;

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

        PlayPickupSound();
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

    private void PlayPickupSound()
    {
        AudioClip clip = GetRandomClip(pickupClips);
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, transform.position, pickupVolume);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }
}
