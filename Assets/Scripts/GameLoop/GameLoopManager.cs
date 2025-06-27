using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    public GameManager gameManager;
    public PickableSpawner spawner;

    [SerializeField] private CinemachineTargetGroup targetGroup;

    public List<GameObject> players;

    [SerializeField] private Transform[] spawnPoints;

    void Awake()
    {
        gameManager = GameManager.Instance;

        gameManager.gameLoopManager = this;
        gameManager.Spawner = spawner;
        gameManager.SetState(new WaitingState());
    }

    private void Start()
    {
        if (gameManager.currentState is WaitingState)
        {
            FindAndAddPlayers();
        }
    }

    public void FindAndAddPlayers()
    {
        PlayerController[] foundPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var pc in foundPlayers)
        {
            GameObject playerObj = pc.gameObject;

            if (!players.Contains(playerObj))
            {
                players.Add(playerObj);
                playerObj.GetComponent<PlayerController>().gameLoopManager = this;
                if (targetGroup != null)
                {
                    targetGroup.AddMember(playerObj.transform, 1, 2);
                }
            }
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Respawn] No spawn points set.");
            return Vector3.zero; // Fallback
        }

        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }

}
