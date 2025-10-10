using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // One pool per prefab
    private readonly Dictionary<int, Pool> _pools = new Dictionary<int, Pool>();

    [System.Serializable]
    private class Pool
    {
        public GameObject Prefab;
        public Transform Root;           // parent container in hierarchy
        public int MaxSize;
        public Queue<GameObject> Free = new Queue<GameObject>();
    }

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            Root = new GameObject($"[Pool] {prefab.name}").transform,
            MaxSize = Mathf.Max(1, maxSize)
        };
        DontDestroyOnLoad(pool.Root.gameObject);

        _pools.Add(key, pool);

        for (int i = 0; i < prewarm; i++)
        {
            var go = Instantiate(prefab, pool.Root);
            go.SetActive(false);
            pool.Free.Enqueue(go);
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!prefab)
        {
            Debug.LogError("PoolManager.Spawn: prefab is null");
            return null;
        }

        int key = prefab.GetInstanceID();
        if (!_pools.TryGetValue(key, out var pool))
        {
            // Auto-register if not registered yet
            RegisterPrefab(prefab);
            pool = _pools[key];
        }

        GameObject go = pool.Free.Count > 0 ? pool.Free.Dequeue() : Instantiate(prefab);
        if (go.transform.parent != parent) go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(position, rotation);

        // Ensure inactive → active to trigger OnEnable if used
        if (!go.activeSelf) go.SetActive(true);

        // Notify IPoolable
        if (go.TryGetComponent<IPoolable>(out var ip)) ip.OnSpawnedFromPool();

        return go;
    }

    /// <summary>
    /// Return an instance to its pool. If pool is full, Destroy.
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (!instance) return;

        // Find original prefab by checking a hidden tag component
        var tag = instance.GetComponent<_PoolTag>();
        if (!tag || !_pools.TryGetValue(tag.PrefabId, out var pool))
        {
            // If object doesn't have a tag (created before pool), try to infer; else destroy
            Destroy(instance);
            return;
        }

        if (instance.TryGetComponent<IPoolable>(out var ip))
            ip.OnDespawnedToPool();

        // Over-cap? destroy surplus; else enqueue
        if (pool.Free.Count >= pool.MaxSize)
        {
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(pool.Root, false);
        pool.Free.Enqueue(instance);
    }

    /// <summary>
    /// Try to despawn if the object came from a pool. Returns true if pooled.
    /// </summary>
    public static bool TryDespawn(GameObject instance)
    {
        if (!Instance) return false;
        var tag = instance ? instance.GetComponent<_PoolTag>() : null;
        if (!tag) return false;
        Instance.Despawn(instance);
        return true;
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
