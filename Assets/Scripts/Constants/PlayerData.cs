using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public ulong clientId;
    public string playerName;
    public GameObject playerPrefab;
    public PlayerDataNet ToNetData()
    {
        return new PlayerDataNet
        {
            clientId = clientId,
            playerName = playerName
        };
    }
}
