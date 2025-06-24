using System.Collections.Generic;
using UnityEngine;

public static class SORegistry
{
    private static readonly Dictionary<System.Type, object> registries = new();

    public static void RegisterAll<T>(string path) where T : ScriptableObject, IIdentifiableSO
    {
        var dict = new Dictionary<int, T>();
        var assets = Resources.LoadAll<T>(path);
        foreach (var asset in assets)
        {
            dict[asset.Id] = asset;
        }
        registries[typeof(T)] = dict;
    }

    public static T Get<T>(int id) where T : ScriptableObject, IIdentifiableSO
    {
        if (registries.TryGetValue(typeof(T), out var rawDict))
        {
            var dict = rawDict as Dictionary<int, T>;
            if (dict != null && dict.TryGetValue(id, out var result))
                return result;
        }
        Debug.LogWarning($"[{typeof(T).Name}] Not found for ID={id}");
        return null;
    }
}
