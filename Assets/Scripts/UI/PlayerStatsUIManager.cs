using UnityEngine;
using System.Collections.Generic;

public class PlayerStatsUIManager : MonoBehaviour
{
    [SerializeField] private GameObject playerStatsUIPrefab;
    [SerializeField] private float margin = 10f;

    private Dictionary<ulong, PlayerStatsUIElement> uiMap = new Dictionary<ulong, PlayerStatsUIElement>();
    private List<NetworkStatsController> statsList = new List<NetworkStatsController>();

    void Awake()
    {
        NetworkStatsController.OnStatsSpawned += HandleStatsSpawn;
        NetworkStatsController.OnStatsDespawned += HandleStatsDespawn;
    }

    void OnDestroy()
    {
        NetworkStatsController.OnStatsSpawned -= HandleStatsSpawn;
        NetworkStatsController.OnStatsDespawned -= HandleStatsDespawn;
    }

    private void HandleStatsSpawn(NetworkStatsController stats)
    {
        statsList.Add(stats);
        RearrangeUI();
    }

    private void HandleStatsDespawn(NetworkStatsController stats)
    {
        statsList.Remove(stats);
        RearrangeUI();
    }

    private void RearrangeUI()
    {
        foreach (var kv in uiMap)
        {
            kv.Value.Cleanup();
            Destroy(kv.Value.gameObject);
        }
        uiMap.Clear();

        int count = statsList.Count;
        for (int i = 0; i < count; i++)
        {
            var stats = statsList[i];
            var go = Instantiate(playerStatsUIPrefab, transform);
            var rt = go.GetComponent<RectTransform>();
            var uiElem = go.GetComponent<PlayerStatsUIElement>();
            uiElem.Initialize(stats);
            PositionUI(rt, i, count);
            uiMap[stats.OwnerClientId] = uiElem;
        }
    }

    private void PositionUI(RectTransform rt, int index, int count)
    {
        Vector2 anchor = Vector2.zero;
        Vector2 pivot = Vector2.zero;
        Vector2 pos = Vector2.zero;

        switch (count)
        {
            case 2:
                anchor = pivot = (index == 0)
                    ? new Vector2(0, 1)  // top-left
                    : new Vector2(1, 1); // top-right
                pos = (index == 0)
                    ? new Vector2(margin, -margin)
                    : new Vector2(-margin, -margin);
                break;

            case 3:
                if (index < 2)
                {
                    anchor = pivot = (index == 0)
                        ? new Vector2(0, 1)
                        : new Vector2(1, 1);
                    pos = (index == 0)
                        ? new Vector2(margin, -margin)
                        : new Vector2(-margin, -margin);
                }
                else
                {
                    anchor = pivot = new Vector2(0, 0);
                    pos = new Vector2(margin, margin);
                }
                break;

            case 4:
                switch (index)
                {
                    case 0:
                        anchor = pivot = new Vector2(0, 1);
                        pos = new Vector2(margin, -margin);
                        break;
                    case 1:
                        anchor = pivot = new Vector2(1, 1);
                        pos = new Vector2(-margin, -margin);
                        break;
                    case 2:
                        anchor = pivot = new Vector2(0, 0);
                        pos = new Vector2(margin, margin);
                        break;
                    case 3:
                    default:
                        anchor = pivot = new Vector2(1, 0);
                        pos = new Vector2(-margin, margin);
                        break;
                }
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