using UnityEngine;

public interface IPoolable
{
    // Called right after the object is (re)spawned from the pool.
    void OnSpawnedFromPool();

    // Called right before the object is returned to the pool.
    void OnDespawnedToPool();
}