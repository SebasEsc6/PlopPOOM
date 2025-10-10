using UnityEngine;

public class PooledBehaviour : MonoBehaviour, IPoolable
{
    public virtual void OnSpawnedFromPool() { }
    public virtual void OnDespawnedToPool() { }

    public void ReturnToPool() => PoolManager.Instance.Despawn(gameObject);
}