using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LobbyBaseNames
{
    public List<string> baseNames;
}

public static class LobbyUtils
{
    private static readonly List<string> lobbyNames;

    static LobbyUtils()
    {
        var textAsset = Resources.Load<TextAsset>("LobbyBaseNames");
        if (textAsset != null)
        {
            var wrapper = JsonUtility.FromJson<LobbyBaseNames>(textAsset.text);
            lobbyNames = wrapper.baseNames;
        }
        else
        {
            lobbyNames = new List<string> { "DefaultLobby" };
            Debug.LogWarning("LobbyBaseNames.json not found in Resources folder.");
        }
    }

    /// <summary>
    /// Generate a random lobby name: base name + numeric suffix
    /// </summary>
    public static string GenerateLobbyName()
    {
        int index = UnityEngine.Random.Range(0, lobbyNames.Count);
        string baseName = lobbyNames[index];
        int suffix = UnityEngine.Random.Range(1, 10000);
        return $"{baseName}{suffix}";
    }

    /// <summary>
    /// Generate a random player name with Ploop_ prefix and 8-char ID
    /// /// </summary>
    public static string GeneratePlayerName()
    {
        string id = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"Ploop_{id}";
    }
}
