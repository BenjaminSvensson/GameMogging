using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerKeyInventory : MonoBehaviour
{
    [SerializeField] private List<string> startingKeyIds = new List<string>();
    [SerializeField] private UnityEvent<string> onKeyAdded;

    private readonly HashSet<string> keyIds = new HashSet<string>();

    private void Awake()
    {
        for (int i = 0; i < startingKeyIds.Count; i++)
        {
            AddKey(startingKeyIds[i]);
        }
    }

    public bool HasKey(string keyId)
    {
        return !string.IsNullOrWhiteSpace(keyId) && keyIds.Contains(NormalizeKeyId(keyId));
    }

    public bool AddKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        string normalizedKeyId = NormalizeKeyId(keyId);
        bool added = keyIds.Add(normalizedKeyId);

        if (added)
        {
            onKeyAdded?.Invoke(normalizedKeyId);
        }

        return added;
    }

    private string NormalizeKeyId(string keyId)
    {
        return keyId.Trim();
    }
}
