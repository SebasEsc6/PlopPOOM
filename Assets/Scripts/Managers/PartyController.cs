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
    

    private void Start()
    {
        SpawnPlayers();
        StartCoroutine(StartCountdownRoutine());
    }

    private void FixedUpdate() {
        CheckKills();
        CheckPlayers();
        
        timer += Time.deltaTime;
        if(timer >= ammoCDRespawn)
        {
            SpawnItems(itemsPrefabs[Random.Range(0,itemsPrefabs.Count)], ammoLifeTime);
            timer = 0;
        }
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

        if (hasStarted)
        {
            _player1Go.GetComponent<EventController>().canControl = true;
            _player2Go.GetComponent<EventController>().canControl = true;
        }
    }

    private void CheckKills()
    {
        if(player1Kills >= 3)
        {
            StartCoroutine(PauseDelay(greenWinsUI));
        }
        if(player2Kills >= 3)
        {
            StartCoroutine(PauseDelay(redWinsUI));
        }
    }

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
