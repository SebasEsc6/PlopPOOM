using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [Header("Players Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [Header("Items Prefabs")]
    [SerializeField] private GameObject ammoPrefab;

    [Header("Items Values")]
    [SerializeField] private float ammoLifeTime;
    [SerializeField] private float ammoCDRespawn;
    private float timer;

    [Header("Kills values")]
    private int _killAmount;
    public int player1Kills;
    public int player2Kills;

    [SerializeField] private GameObject redWinsUI;
    [SerializeField] private GameObject greenWinsUI;

    [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup;

    [Header("SpawnPoints")]
    [SerializeField] private float reSpawnCd;
    [SerializeField] private List<GameObject> initialSpawnPoints;
    [SerializeField] private List<GameObject> reSpawnPoints;
    [SerializeField] private List<GameObject> itemSpawns;

    //Objects & scripts references
    private GameObject _player1Go;
    private GameObject _player2Go;

    public StatsController _player1Stats;
    public StatsController _player2Stats;
    public float timeToPause;

    private void Start() {
        SpawnPlayers();
    }
    private void FixedUpdate() {
        CheckKills();
        CheckPlayers();
        
        timer += Time.deltaTime;
        if(timer >=ammoCDRespawn )
        {
            SpawnItems(ammoPrefab, ammoLifeTime);
            timer = 0;
        }
    }

    //Initial Spawn of the players and set references
    public void SpawnPlayers()
    {
        _player1Go = Instantiate(player1Prefab, initialSpawnPoints[0].transform.position, Quaternion.identity);
        _player2Go = Instantiate(player2Prefab, initialSpawnPoints[1].transform.position, Quaternion.identity);

        _player1Stats = _player1Go.GetComponent<StatsController>();
        _player2Stats = _player2Go.GetComponent<StatsController>();

        cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
        cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
    }
    private void CheckKills()
    {
        if(player1Kills >= 3)
        {
            StartCoroutine(ActiveFeedbackWithDelay(greenWinsUI));
            
        }
        if(player2Kills >= 3)
        {
            StartCoroutine(ActiveFeedbackWithDelay(redWinsUI));
        }
        
    }

    //Check if one player is die for reespawn
    public void CheckPlayers()
    {   
        if (player1Kills >= 3 || player2Kills >= 3)
        {
            return;
        }
        // Check if player1 is dead
        if (_player1Stats.isDie)
        {
            // Increment player2's kill count as player1 died
            player2Kills++;

            // Only respawn player1 if player2's kills are less than 3
            if (player2Kills < 3)
            {
                _player1Go = ReSpawnPlayer(player1Prefab);
                _player1Go.SetActive(false);
                StartCoroutine(SpawnDelay(_player1Go));
                _player1Stats = _player1Go.GetComponent<StatsController>();
                cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
            }
        }

        // Check if player2 is dead
        if (_player2Stats.isDie)
        {
            // Increment player1's kill count as player2 died
            player1Kills++;

            // Only respawn player2 if player1's kills are less than 3
            if (player1Kills < 3)
            {
                _player2Go = ReSpawnPlayer(player2Prefab);
                _player2Go.SetActive(false);
                StartCoroutine(SpawnDelay(_player2Go));
                _player2Stats = _player2Go.GetComponent<StatsController>();
                cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
            }
        }

        // Update the total kill count (if this is used en otro lado)
        _killAmount = player1Kills + player2Kills;
        
    }

    IEnumerator SpawnDelay(GameObject obj)
    {
        // Wait for the respawn cooldown time
        yield return new WaitForSeconds(reSpawnCd);
        obj.SetActive(true);
        // If any player already reached 3 kills, do not respawn anyone
        // if (player1Kills >= 3 || player2Kills >= 3)
        // {
        //     yield break;
        // }

        // // Check if player1 is dead
        // if (_player1Stats.isDie)
        // {
        //     // Increment player2's kill count as player1 died
        //     player2Kills++;

        //     // Only respawn player1 if player2's kills are less than 3
        //     if (player2Kills < 3)
        //     {
        //         _player1Go = ReSpawnPlayer(player1Prefab);
        //         _player1Stats = _player1Go.GetComponent<StatsController>();
        //         cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
        //     }
        // }

        // // Check if player2 is dead
        // if (_player2Stats.isDie)
        // {
        //     // Increment player1's kill count as player2 died
        //     player1Kills++;

        //     // Only respawn player2 if player1's kills are less than 3
        //     if (player1Kills < 3)
        //     {
        //         _player2Go = ReSpawnPlayer(player2Prefab);
        //         _player2Stats = _player2Go.GetComponent<StatsController>();
        //         cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
        //     }
        // }

        // // Update the total kill count (if this is used en otro lado)
        // _killAmount = player1Kills + player2Kills;
    }


    private IEnumerator ActiveFeedbackWithDelay(GameObject playerWinUI)
    {
        playerWinUI.SetActive(true);
        yield return new WaitForSeconds(timeToPause);
        Time.timeScale = 0;
    }

    //Instantiate a especific prefab player
    private GameObject ReSpawnPlayer(GameObject playerPrefab)
    {
        return Instantiate(playerPrefab, SetSpawn(reSpawnPoints).transform.position, Quaternion.identity);
    }

    //Get a random spawn point of especific list of spawnpoints
    private GameObject SetSpawn(List<GameObject> type)
    {
        return type[Random.Range(0, type.Count)];
    }

    private void SpawnItems(GameObject item, float timeToDestroy)
    {
        var spawn = SetSpawn(itemSpawns);
        var spawnedItem = Instantiate(item, spawn.transform.position, Quaternion.identity);
        Destroy(spawnedItem, timeToDestroy);
    }
}
