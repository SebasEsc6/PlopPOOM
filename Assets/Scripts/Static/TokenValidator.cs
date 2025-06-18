using System.Collections.Generic;
using UnityEngine;

public static class TokenValidator
{
    private static readonly Dictionary<object, object> registry = new();

    public static void Register<TKey, TToken>(TKey key, TToken token)
    {
        registry[key] = token;
    }

    public static bool ValidateAndConsume<TKey, TToken>(TKey key, TToken token)
    {
        Debug.Log($"[TokenValidator] Checking key {key} with token {token}");

        if (registry.TryGetValue(key, out var stored) && EqualityComparer<TToken>.Default.Equals((TToken)stored, token))
        {
            Debug.Log($"[TokenValidator] Token valid. Consuming...");
            registry.Remove(key);
            return true;
        }

        Debug.LogWarning($"[TokenValidator] Invalid or missing token for key {key}");
        return false;
    }

    public static void ClearAll() => registry.Clear();
}
