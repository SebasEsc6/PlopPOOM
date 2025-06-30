using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionUI : MonoBehaviour
{

    [SerializeField] private int prefabHash = 0;

    private void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => OnPlayerOptionClicked(prefabHash));
        }
    }

    public void OnPlayerOptionClicked(int prefabHash)
    {
        Debug.Log($"[PlayerSelectionUI] Player option clicked: {prefabHash}");
        // Any client can call this, ServerRpc will update the NetworkList
        LobbyDataManager.Instance.SetPrefabHashServerRpc(prefabHash);
    }
}