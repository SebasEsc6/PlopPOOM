using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerStatsUIManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject playerStatsUIPrefab;
    [SerializeField] private float margin = 10f;

    private readonly List<NetworkStatsController> controllers = new();
    private readonly Dictionary<ulong, PlayerStatsUIElement> uiElements = new();

    private void OnEnable()
    {
        NetworkStatsController.OnStatsSpawned += HandleStatsCtrlSpawn;
        NetworkStatsController.OnStatsDespawned += HandleStatsCtrlDespawn;
    }

    private void OnDisable()
    {
        NetworkStatsController.OnStatsSpawned -= HandleStatsCtrlSpawn;
        NetworkStatsController.OnStatsDespawned += HandleStatsCtrlDespawn;
    }

    private void HandleStatsCtrlSpawn(NetworkStatsController statsCtrl)
    {
        AddController(statsCtrl);
        RepositionAllElements();
    }

    private void HandleStatsCtrlDespawn(NetworkStatsController statsCtrl)
    {
        AddController(statsCtrl);
        RepositionAllElements();
    }

    private void AddController(NetworkStatsController ctrl)
    {
        if (uiElements.ContainsKey(ctrl.OwnerClientId)) return;

        controllers.Add(ctrl);

        var go = Instantiate(playerStatsUIPrefab, transform);
        var ui = go.GetComponent<PlayerStatsUIElement>();

        ui.Bind(ctrl);
        uiElements[ctrl.OwnerClientId] = ui;

        RepositionAllElements();
    }

    private void RepositionAllElements()
    {
        int total = controllers.Count;
        for (int i = 0; i < total; i++)
        {
            var ctrl = controllers[i];
            if (uiElements.TryGetValue(ctrl.OwnerClientId, out var ui))
            {
                var rt = ui.GetComponent<RectTransform>();
                PositionUIElement(rt, i, total);
            }
        }
    }

    private void PositionUIElement(RectTransform rt, int index, int count)
    {
        Vector2 anchor, pivot, pos;
        switch (count)
        {
            case 2:
                anchor = pivot = index == 0 ? new Vector2(0, 1) : new Vector2(1, 1);
                pos = index == 0 ? new Vector2(margin, -margin) : new Vector2(-margin, -margin);
                break;
            case 3:
                if (index < 2)
                {
                    anchor = pivot = index == 0 ? new Vector2(0, 1) : new Vector2(1, 1);
                    pos = index == 0 ? new Vector2(margin, -margin) : new Vector2(-margin, -margin);
                }
                else
                {
                    anchor = pivot = new Vector2(0, 0);
                    pos = new Vector2(margin, margin);
                }
                break;
            case 4:
                anchor = pivot = new Vector2(index % 2, index < 2 ? 1 : 0);
                pos = new Vector2(index % 2 == 0 ? margin : -margin,
                                     index < 2 ? -margin : margin);
                break;
            default:
                float step = 1f / (count + 1);
                anchor = pivot = new Vector2(step * (index + 1), 1);
                pos = new Vector2(0, -margin);
                break;
        }
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
    }
}
