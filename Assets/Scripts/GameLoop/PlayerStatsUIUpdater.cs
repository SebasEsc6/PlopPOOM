using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerStatsUIUpdater : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManager;

    // Puedes referenciar tus elementos de UI aquí (por jugador)
    // por ejemplo: Dictionary<ulong, TMP_Text> playerKillTexts;

    private void Start()
    {
        if (gameLoopManager == null)
        {
            gameLoopManager = GameManager.Instance?.gameLoopManager;
        }

        if (gameLoopManager != null)
        {
            gameLoopManager.playerStatsList.OnListChanged += OnStatsChanged;
            RenderStats();
        }
    }

    private void OnDestroy()
    {
        if (gameLoopManager != null)
        {
            gameLoopManager.playerStatsList.OnListChanged -= OnStatsChanged;
        }
    }

    private void OnStatsChanged(NetworkListEvent<PlayerStats> change)
    {
        RenderStats();
    }

    private void RenderStats()
    {
        if (gameLoopManager == null) return;
        //This is a test for get stats from a specific clientId
        if (gameLoopManager.TryGetPlayerStats(NetworkManager.Singleton.LocalClientId, out var myStats))
        {
            Debug.Log($"Mis stats → Kills: {myStats.kills} | Lives: {myStats.lives}");
        }


        foreach (var stats in gameLoopManager.playerStatsList)
        {
            Debug.Log($"Client {stats.clientId} → Kills: {stats.kills}, Lives: {stats.lives}");

            // Aquí actualizas tu UI visualmente, por ejemplo:
            // playerKillTexts[stats.clientId].text = $"Kills: {stats.kills}";
            // playerLifeTexts[stats.clientId].text = $"Lives: {stats.lives}";
        }
    }

    
}
