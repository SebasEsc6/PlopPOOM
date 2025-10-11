using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [Header("Players Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [Header("Items Prefabs")]
    [SerializeField] private List<GameObject> itemsPrefabs;

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

    // Timer para el inicio
    [Header("Start Timer")]
    [SerializeField] private float startCountdown = 3f;
    [SerializeField] private TMP_Text countdownText;
    public bool hasStarted = false;

    [Header("Player References")]

    //Objects & scripts references
    private GameObject _player1Go;
    private GameObject _player2Go;

    public StatsController _player1Stats;
    public StatsController _player2Stats;

    public MovementController _player1Movement;
    public MovementController _player2Movement;
    public float timeToPause;

    private bool _p1DeathHandled;
    private bool _p2DeathHandled;
    

    private void Start()
    {
        SpawnPlayers();
        StartCoroutine(StartCountdownRoutine());
    }

    private void FixedUpdate() {
        // Count win condition every frame (still OK)
        // CheckKills();

        // --- CHANGED: handle death edge-triggers immediately
        HandleDeaths();
        
        timer += Time.deltaTime;
        if(timer >= ammoCDRespawn)
        {
            float value = Random.Range(0f,1f);
            if(value <= 0.65f)
            {
                SpawnItems(itemsPrefabs[0], ammoLifeTime);
            }
            else
            {
                SpawnItems(itemsPrefabs[1], ammoLifeTime);
            }
            timer = 0;
        }
    }
     private void HandleDeaths()
    {
        // Guard clauses if references haven't been set yet
        if (_player1Stats != null)
        {
            // If player 1 has just died and we haven't handled it yet
            if (_player1Stats.isDie && !_p1DeathHandled)
            {
                _p1DeathHandled = true; // mark handled

                // --- Count the kill immediately (player 2 gets a kill)
                player2Kills++;

                // --- Update win state right away after counting
                CheckKills();

                // --- Start respawn coroutine for player 1
                StartCoroutine(RespawnAfterDelay(1));
            }
        }

        if (_player2Stats != null)
        {
            // If player 2 has just died and we haven't handled it yet
            if (_player2Stats.isDie && !_p2DeathHandled)
            {
                _p2DeathHandled = true; // mark handled

                // --- Count the kill immediately (player 1 gets a kill)
                player1Kills++;

                // --- Update win state right away after counting
                CheckKills();

                // --- Start respawn coroutine for player 2
                StartCoroutine(RespawnAfterDelay(2));
            }
        }
    }

     private IEnumerator RespawnAfterDelay(int playerIndex)
    {
        // Wait for the configured respawn cooldown
        yield return new WaitForSeconds(reSpawnCd);

        if (playerIndex == 1)
        {
            // If still dead (defensive), respawn player 1
            if (_player1Stats == null || _player1Stats.isDie)
            {
                // Remove old target if needed (optional: to avoid piling up)
                // cinemachineTargetGroup.RemoveMember(_player1Go.transform);

                _player1Go = ReSpawnPlayer(player1Prefab);
                _player1Stats = _player1Go.GetComponent<StatsController>();
                cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);

                // Reset one-shot flag for next death cycle
                _p1DeathHandled = false;
            }
        }
        else if (playerIndex == 2)
        {
            if (_player2Stats == null || _player2Stats.isDie)
            {
                // cinemachineTargetGroup.RemoveMember(_player2Go.transform);

                _player2Go = ReSpawnPlayer(player2Prefab);
                _player2Stats = _player2Go.GetComponent<StatsController>();
                cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);

                _p2DeathHandled = false;
            }
        }

        // Optional: recompute total kills
        _killAmount = player1Kills + player2Kills;
    }

    private IEnumerator StartCountdownRoutine()
    {
        _player1Go.GetComponent<EventController>().canControl = false;
        _player2Go.GetComponent<EventController>().canControl = false;

        float timeLeft = startCountdown;

        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();

            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        hasStarted = true;

        _player1Go.GetComponent<EventController>().canControl = true;
        _player2Go.GetComponent<EventController>().canControl = true;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
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

        // Reset one-shot flags on fresh spawns
        _p1DeathHandled = false;
        _p2DeathHandled = false;

        if (hasStarted)
        {
            _player1Go.GetComponent<EventController>().canControl = true;
            _player2Go.GetComponent<EventController>().canControl = true;
        }
    }

    private void CheckKills()
    {
        if(player1Kills >= 1)
        {
            StartCoroutine(PauseDelay(redWinsUI));
        }
        if(player2Kills >= 1)
        {
            StartCoroutine(PauseDelay(greenWinsUI));
        }
    }

    // public void CheckPlayers()
    // {
    //     if(_player1Stats.isDie || _player2Stats.isDie)
    //     {
    //         StartCoroutine(SpawnDelay());
    //     }
    // }

    // IEnumerator SpawnDelay()
    // {
    //     yield return new WaitForSeconds(reSpawnCd);
    //     if(_player1Stats.isDie)
    //     {
    //         player2Kills ++;
    //         _player1Go = ReSpawnPlayer(player1Prefab);
    //         _player1Stats = _player1Go.GetComponent<StatsController>();
    //         cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
    //     }

    //     if (_player2Stats.isDie)
    //     {
    //         player1Kills ++;
    //         _player2Go = ReSpawnPlayer(player2Prefab);
    //         _player2Stats = _player2Go.GetComponent<StatsController>();
    //         cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
    //     }
    //     _killAmount = player1Kills + player2Kills;
    // }

    private IEnumerator PauseDelay(GameObject playerWinUI)
    {
        playerWinUI.SetActive(true);
        yield return new WaitForSeconds(timeToPause);
        Time.timeScale = 0;
    }

    private GameObject ReSpawnPlayer(GameObject playerPrefab)
    {
        return Instantiate(playerPrefab, SetSpawn(reSpawnPoints).transform.position, Quaternion.identity);
    }

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
