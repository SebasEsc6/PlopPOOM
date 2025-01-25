using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [Header("Players Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [SerializeField] private float reSpawnCd;
    private GameObject _player1Go;
    private GameObject _player2Go;

    private StatsController _player1Stats;
    private StatsController _player2Stats;


    
    [Header("SpawnPoints")]
    [SerializeField] private List<GameObject> initialSpawnPoints;
    [SerializeField] private List<GameObject> reSpawnPoints;

    private void Start() {
        SpawnPlayers();
    }
    private void FixedUpdate() {
        CheckPlayers();
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
            _player1Go = ReSpawnPlayer(player1Prefab);
            _player1Stats = _player1Go.GetComponent<StatsController>();
        }

        if (_player2Stats.isDie)
        {
            _player2Go = ReSpawnPlayer(player2Prefab);
            _player2Stats = _player2Go.GetComponent<StatsController>();
        }
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
}
