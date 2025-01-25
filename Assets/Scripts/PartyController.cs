using System.Collections;
using System.Collections.Generic;
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

    [Header("Kills values")]
    private int _killAmount;
    public int player1Kills;
    public int player2Kills;

    [Header("SpawnPoints")]
    [SerializeField] private float reSpawnCd;
    [SerializeField] private List<GameObject> initialSpawnPoints;
    [SerializeField] private List<GameObject> reSpawnPoints;
    [SerializeField] private List<GameObject> itemSpawns;

    //Objects & scripts references
    private GameObject _player1Go;
    private GameObject _player2Go;

    private StatsController _player1Stats;
    private StatsController _player2Stats;

    private void Start() {
        SpawnPlayers();
    }
    private void FixedUpdate() {
        CheckPlayers();

        if(_killAmount > 0)
        {
            StartCoroutine(SpawnItems(ammoPrefab, ammoCDRespawn, ammoLifeTime));
        }
    }

    //Initial Spawn of the players and set references
    public void SpawnPlayers()
    {
        _player1Go = Instantiate(player1Prefab, initialSpawnPoints[0].transform.position, Quaternion.identity);
        _player2Go = Instantiate(player2Prefab, initialSpawnPoints[1].transform.position, Quaternion.identity);

        _player1Stats = _player1Go.GetComponent<StatsController>();
        _player2Stats = _player2Go.GetComponent<StatsController>();
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
        //TODO: CHANGE THE SCORE
        Debug.Log("One player is dead and is");
        yield return new WaitForSeconds(reSpawnCd);
        if(_player1Stats.isDie)
        {
            player2Kills ++;
            _player1Go = ReSpawnPlayer(player1Prefab);
            _player1Stats = _player1Go.GetComponent<StatsController>();
        }

        if (_player2Stats.isDie)
        {
            player1Kills ++;
            _player2Go = ReSpawnPlayer(player2Prefab);
            _player2Stats = _player2Go.GetComponent<StatsController>();
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

    private IEnumerator SpawnItems(GameObject item, float timeToSpawn, float timeToDestroy)
    {
        yield return new WaitForSeconds(timeToSpawn);
        var spawn = SetSpawn(itemSpawns);
        var spawnedItem = Instantiate(item, spawn.transform.position, Quaternion.identity);
        Destroy(spawnedItem, timeToDestroy);
    }
}
