using UnityEngine;

public class MapSelectionUI : MonoBehaviour
{
    public void OnMapButtonClicked(int mapIndex)
    {
        if (LobbyDataManager.Instance.IsServer)
        {
            LobbyDataManager.Instance.ChooseMap(mapIndex);
        }
    }
}