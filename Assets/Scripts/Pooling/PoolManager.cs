using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("Scene Root for pooled objects")]
    [SerializeField] private Transform poolRoot;

    [System.Serializable]
    private class Pool
    {
        public GameObject Prefab;
        public Transform Root;           // parent container in hierarchy
        public int MaxSize;
        public Queue<GameObject> Free = new Queue<GameObject>();
        public int PrefabKey;
    }

    private readonly Dictionary<int, Pool> _pools = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!poolRoot)
        {
            var rootGO = new GameObject("[PoolRoot]");
            poolRoot = rootGO.transform;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void RegisterPrefab(GameObject prefab, int prewarm = 0, int maxSize = 64)
    {
        if (!prefab) { Debug.LogError("PoolManager.RegisterPrefab: null prefab"); return; }

        int key = prefab.GetInstanceID();
        if (_pools.ContainsKey(key)) return;

        var pool = new Pool
        {
            Prefab = prefab,
            PrefabKey = key,
            Root = new GameObject($"[Pool] {prefab.name}").transform,
            MaxSize = Mathf.Max(1, maxSize)
        };

        pool.Root.SetParent(poolRoot, false);

        _pools.Add(key, pool);

        for (int i = 0; i < prewarm; i++)
        {
            var go = Instantiate(prefab, pool.Root);
            go.SetActive(false);
            pool.Free.Enqueue(go);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (prefab == null) { Debug.LogError("[Pool] prefab null"); return null; }

        // get/create pool for this prefab...
        var pool = GetOrCreatePool(prefab);

        GameObject go = pool.Free.Count > 0 ? pool.Free.Dequeue()
                                            : Instantiate(prefab);  // ← not Destroy, not scene instance

        // parent + pose
        if (go.transform.parent != parent) go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot);

        // activate BEFORE calling hooks so components can run coroutines
        if (!go.activeSelf) go.SetActive(true);

        var poolables = go.GetComponents<IPoolable>();
        for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawnedFromPool();

        return go;
    }

    /// <summary>
    /// Return an instance to its pool. If pool is full, Destroy.
    /// </summary>
    public void Despawn(GameObject go)
    {
        if (!go) return;

        foreach (var p in go.GetComponents<IPoolable>()) p.OnDespawnedToPool();

        var pool = FindOwningPool(go);
        if (pool != null)
        {
            go.transform.SetParent(pool.Root, false);
            go.SetActive(false);
            pool.Free.Enqueue(go);
        }
        else
        {
            go.transform.SetParent(poolRoot, false);
            go.SetActive(false);
            Debug.LogWarning("[Pool] object had no pool; parking under poolRoot.");
        }
    }

    /// <summary>
    /// Try to despawn if the object came from a pool. Returns true if pooled.
    /// </summary>
    public static bool TryDespawn(GameObject go)
    {
        if (!Instance || !go) return false;
        Instance.Despawn(go);
        return true;
    }

    private Pool GetOrCreatePool(GameObject prefab)
    {
        int key = prefab.GetInstanceID();
        if (_pools.TryGetValue(key, out var pool)) return pool;

        // Create new pool on-the-fly with default params
        RegisterPrefab(prefab, prewarm: 0, maxSize: 64);
        return _pools[key];
    }

    private Pool FindOwningPool(GameObject instance)
    {
        var tag = instance.GetComponent<_PoolTag>();
        if (tag == null) return null;

        int prefabId = tag.PrefabId;
        if (_pools.TryGetValue(prefabId, out var pool)) return pool;

        return null;
    }

    // Attach this to instances at creation time (first instantiate) to remember their prefab.
    private class _PoolTag : MonoBehaviour
    {
        public int PrefabId;
    }

    // Ensure any new Instantiate through this PoolManager adds a _PoolTag
    private GameObject Instantiate(GameObject prefab)
    {
        var go = Object.Instantiate(prefab);
        var tag = go.GetComponent<_PoolTag>();
        if (!tag) tag = go.AddComponent<_PoolTag>();
        tag.PrefabId = prefab.GetInstanceID();
        return go;
    }

    private GameObject Instantiate(GameObject prefab, Transform parent)
    {
        var go = Object.Instantiate(prefab, parent);
        var tag = go.GetComponent<_PoolTag>();
        if (!tag) tag = go.AddComponent<_PoolTag>();
        tag.PrefabId = prefab.GetInstanceID();
        return go;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _pools.Clear();
    }
}
