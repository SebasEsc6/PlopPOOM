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
    [SerializeField] private GameObject player1AvatarPrefab;
    [SerializeField] private GameObject player2RootPrefab;
    [SerializeField] private GameObject player2AvatarPrefab;

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

    private GameObject _p1Root;
    private GameObject _p2Root;
    private PlayerAvatarHandle _p1Handle;
    private PlayerAvatarHandle _p2Handle;
    public event System.Action<int, StatsController> OnAvatarReady;


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
        yield return new WaitForSeconds(reSpawnCd);

        if (playerIndex == 1)
        {
            // If still dead or avatar missing → respawn avatar from pool at a random respawn point
            if (_player1Stats == null || _player1Stats.isDie || _player1Go == null)
            {
                Vector3 pos = SetSpawn(reSpawnPoints).transform.position;
                Quaternion rot = Quaternion.identity;

                // Despawn old avatar if still around
                if (_player1Go) PoolManager.TryDespawn(_player1Go);

                // Spawn new avatar under Player 1 root
                _p1Handle.SpawnAvatar(pos, rot);

                // Re-cache references
                _player1Go = _p1Handle.CurrentAvatar;
                _player1Stats = _player1Go.GetComponent<StatsController>();
                OnAvatarReady?.Invoke(0, _player1Stats);

                // Re-bind input if needed
                var ec = _player1Go.GetComponent<EventController>();
                ec?.TryBind();

                _p1DeathHandled = false;
            }
        }
        else if (playerIndex == 2)
        {
            if (_player2Stats == null || _player2Stats.isDie || _player2Go == null)
            {
                Vector3 pos = SetSpawn(reSpawnPoints).transform.position;
                Quaternion rot = Quaternion.identity;

                if (_player2Go) PoolManager.TryDespawn(_player2Go);

                _p2Handle.SpawnAvatar(pos, rot);

                _player2Go = _p2Handle.CurrentAvatar;
                _player2Stats = _player2Go.GetComponent<StatsController>();
                OnAvatarReady?.Invoke(1, _player2Stats);

                var ec = _player2Go.GetComponent<EventController>();
                ec?.TryBind();

                _p2DeathHandled = false;
            }
        }

        if (cinemachineTargetGroup != null)
        {
            Transform p1Target = _p1Root.transform.Find("CameraAnchor") ?? _player1Go.transform;
            Transform p2Target = _p2Root.transform.Find("CameraAnchor") ?? _player2Go.transform;
            cinemachineTargetGroup.AddMember(p1Target, 1, 5);
            cinemachineTargetGroup.AddMember(p2Target, 1, 5);
        }

        _killAmount = player1Kills + player2Kills;
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

    private void CheckKills()
    {
        if (player1Kills >= 3)
        {
            StartCoroutine(PauseDelay(redWinsUI));
        }
        if (player2Kills >= 3)
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
