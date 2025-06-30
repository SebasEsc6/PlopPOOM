using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class MapSelectionUI : MonoBehaviour
{
    private List<Button> mapButtons = new List<Button>();
    private LobbyDataManager dataManager;

    private void Start()
    {
        mapButtons.AddRange(GetComponentsInChildren<Button>());
        dataManager = LobbyDataManager.Instance;

        bool isHost = NetworkManager.Singleton.IsServer;
        if (isHost)
        {
            for (int i = 0; i < mapButtons.Count; i++)
            {
                int index = i;
                mapButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"[MapSelectionUI] Host eligió mapa {index}");
                    dataManager.ChooseMap(index);
                });
            }
        }
    }
}
