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

    private void Start() {
        SpawnPlayers();
    }
    private void FixedUpdate() {
        CheckPlayers();
        timer += Time.deltaTime;
        if(_killAmount > 0 && timer >=ammoCDRespawn )
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

    //Check if one player is die for reespawn
    public void CheckPlayers()
    {
        if(_player1Stats.isDie || _player2Stats.isDie)
        {
            StartCoroutine(SpawnDelay());
        }
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(reSpawnCd);
        if(_player1Stats.isDie)
        {
            player2Kills ++;
            _player1Go = ReSpawnPlayer(player1Prefab);
            _player1Stats = _player1Go.GetComponent<StatsController>();
            cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
        }

        if (_player2Stats.isDie)
        {
            player1Kills ++;
            _player2Go = ReSpawnPlayer(player2Prefab);
            _player2Stats = _player2Go.GetComponent<StatsController>();
            cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
        }
        _killAmount = player1Kills + player2Kills;
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
