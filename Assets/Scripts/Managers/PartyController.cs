using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PartyController : MonoBehaviour
{
    [Header("Players Prefabs")]
    [SerializeField] private GameObject player1RootPrefab;
    [SerializeField] private GameObject player2RootPrefab;

    [Header("Items Prefabs")]
    [SerializeField] private List<GameObject> itemsPrefabs;

    [Header("Items Values")]
    [SerializeField] private float ammoLifeTime;
    [SerializeField] private float ammoCDRespawn;
    private float timer;

    [Header("Kills values")]
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

    private GameObject _p1Root;
    private GameObject _p2Root;
    private PlayerAvatarHandle _p1Handle;
    private PlayerAvatarHandle _p2Handle;
    public event System.Action<int, StatsController> OnAvatarReady;
    private bool _p1Respawning, _p2Respawning;

    private void Start()
    {
        SpawnPlayers();
        StartCoroutine(StartCountdownRoutine());
    }

    private void FixedUpdate()
    {
        // Count win condition every frame (still OK)
        // CheckKills();

        // --- CHANGED: handle death edge-triggers immediately
        HandleDeaths();

        timer += Time.deltaTime;
        if (timer >= ammoCDRespawn)
        {
            float value = Random.Range(0f, 1f);
            if (value <= 0.65f)
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

    public void RegisterRoot(PlayerInput pi, int index)
    {
        var root = pi.gameObject;
        var handle = root.GetComponent<PlayerAvatarHandle>();
        if (!handle) handle = root.AddComponent<PlayerAvatarHandle>();

        var sp = initialSpawnPoints[index].transform;
        handle.SpawnAvatar(sp.position, sp.rotation);

        var avatar = handle.CurrentAvatar;
        var stats = avatar.GetComponent<StatsController>();
        var ec = avatar.GetComponent<EventController>(); ec?.TryBind();

        if (index == 0) { _p1Root = root; _p1Handle = handle; _player1Go = avatar; _player1Stats = stats; }
        else { _p2Root = root; _p2Handle = handle; _player2Go = avatar; _player2Stats = stats; }
    }

    //Initial Spawn of the players and set references
    public void SpawnPlayers()
    {
        _p1Root = Instantiate(player1RootPrefab, initialSpawnPoints[0].transform.position, initialSpawnPoints[0].transform.rotation);
        _p2Root = Instantiate(player2RootPrefab, initialSpawnPoints[1].transform.position, initialSpawnPoints[1].transform.rotation);

        _p1Handle = _p1Root.GetComponent<PlayerAvatarHandle>();
        _p2Handle = _p2Root.GetComponent<PlayerAvatarHandle>();

        _p1Handle.SpawnAvatar(initialSpawnPoints[0].transform.position, initialSpawnPoints[0].transform.rotation);
        _player1Stats = _p1Handle.CurrentAvatar.GetComponent<StatsController>();
        OnAvatarReady?.Invoke(0, _player1Stats);
        _p2Handle.SpawnAvatar(initialSpawnPoints[1].transform.position, initialSpawnPoints[1].transform.rotation);
        _player2Stats = _p2Handle.CurrentAvatar.GetComponent<StatsController>();
        OnAvatarReady?.Invoke(1, _player2Stats);

        _player1Go = _p1Handle.CurrentAvatar;
        _player2Go = _p2Handle.CurrentAvatar;
        _player1Stats = _player1Go ? _player1Go.GetComponent<StatsController>() : null;
        _player2Stats = _player2Go ? _player2Go.GetComponent<StatsController>() : null;

        var p1EC = _player1Go ? _player1Go.GetComponent<EventController>() : null;
        var p2EC = _player2Go ? _player2Go.GetComponent<EventController>() : null;

        if (p1EC == null) Debug.LogError("[PartyController] Player1 Avatar is missing EventController.");
        if (p2EC == null) Debug.LogError("[PartyController] Player2 Avatar is missing EventController.");

        p1EC?.TryBind();
        p2EC?.TryBind();

        if (cinemachineTargetGroup != null)
        {
            Transform p1Target = _p1Root.transform.Find("CameraAnchor") ?? _player1Go.transform;
            Transform p2Target = _p2Root.transform.Find("CameraAnchor") ?? _player2Go.transform;
            cinemachineTargetGroup.AddMember(p1Target, 1, 5);
            cinemachineTargetGroup.AddMember(p2Target, 1, 5);
        }

        _p1DeathHandled = _p2DeathHandled = false;

        if (hasStarted)
        {
            if (p1EC != null) p1EC.canControl = true;
            if (p2EC != null) p2EC.canControl = true;
        }
        else
        {
            if (p1EC != null) p1EC.canControl = false;
            if (p2EC != null) p2EC.canControl = false;
        }
    }

    private IEnumerator RespawnAfterDelay(int playerIndex)
    {
        if (playerIndex == 1)
        {
            if (_p1Respawning) yield break;  
            _p1Respawning = true;

            yield return new WaitForSeconds(reSpawnCd);

            var pos = SetSpawn(reSpawnPoints).transform.position;
            _p1Handle.SpawnAvatar(pos, Quaternion.identity);

            _player1Go = _p1Handle.CurrentAvatar;
            cinemachineTargetGroup.AddMember(_player1Go.transform, 1, 5);
            _player1Stats = _player1Go.GetComponent<StatsController>();
            OnAvatarReady?.Invoke(0, _player1Stats);
            
            var ec1 = _player1Go.GetComponent<EventController>();
            ec1?.TryBind();
            ec1.canControl = true;

            _p1DeathHandled = false;   
            _p1Respawning = false;
        }
        else if (playerIndex == 2)
        {
            if (_p2Respawning) yield break;
            _p2Respawning = true;

            yield return new WaitForSeconds(reSpawnCd);

            var pos = SetSpawn(reSpawnPoints).transform.position;
            _p2Handle.SpawnAvatar(pos, Quaternion.identity);

            _player2Go = _p2Handle.CurrentAvatar;
            cinemachineTargetGroup.AddMember(_player2Go.transform, 1, 5);
            _player2Stats = _player2Go.GetComponent<StatsController>();
            OnAvatarReady?.Invoke(1, _player2Stats);

            var ec2 = _player2Go.GetComponent<EventController>();
            ec2?.TryBind();
            ec2.canControl = true;

            _p2DeathHandled = false;
            _p2Respawning = false;
        }
    }

    private IEnumerator StartCountdownRoutine()
    {
        // 👉 Wait until both players and their ECs exist (one frame max usually)
        EventController p1EC = null, p2EC = null;

        // Wait until both GOs are non-null
        while (_player1Go == null || _player2Go == null)
            yield return null;

        // Get ECs (may be missing if prefab wrong)
        p1EC = _player1Go.GetComponent<EventController>();
        p2EC = _player2Go.GetComponent<EventController>();

        // If using tolerant EventController, ensure it’s bound to PlayerInput
        p1EC?.TryBind();
        p2EC?.TryBind();

        // If either EC is missing, abort gracefully (don’t crash the match)
        if (p1EC == null || p2EC == null)
        {
            Debug.LogWarning("[PartyController] Countdown skipped: EventController missing on one of the players.");
            hasStarted = true;
            yield break;
        }

        // Lock controls during countdown
        p1EC.canControl = false;
        p2EC.canControl = false;

        float timeLeft = startCountdown;
        while (timeLeft > 0f)
        {
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();

            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        hasStarted = true;

        // Unlock controls
        p1EC.canControl = true;
        p2EC.canControl = true;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void HandleDeaths()
    {
        if (_player1Stats && _player1Stats.isDie && !_p1DeathHandled)
        {
            _p1DeathHandled = true;
            player2Kills++;
            CheckKills();
            cinemachineTargetGroup.RemoveMember(_player1Go.transform);
            StartCoroutine(RespawnAfterDelay(1));
        }

        if (_player2Stats && _player2Stats.isDie && !_p2DeathHandled)
        {
            _p2DeathHandled = true;
            player1Kills++;
            CheckKills();
            cinemachineTargetGroup.RemoveMember(_player2Go.transform);
            StartCoroutine(RespawnAfterDelay(2));
        }
    }

    private void CheckKills()
    {
        int toWin = MatchConfig.LivesPerPlayer;

        if (player1Kills >= toWin)
        {
            StartCoroutine(PauseDelay(redWinsUI));
        }
        if (player2Kills >= toWin)
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
