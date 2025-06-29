using UnityEngine;

public class PlayerSelectionUI : MonoBehaviour
{
    public void OnPlayerOptionClicked(int prefabHash)
    {
        // Any client can call this, ServerRpc will update the NetworkList
        LobbyDataManager.Instance.SetPrefabHashServerRpc(prefabHash);
    }
}