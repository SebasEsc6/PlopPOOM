using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PickableBase : NetworkBehaviour, IPickable
{
    public SpriteRenderer spriteRenderer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Spawner.Release(gameObject);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NetworkObject.Despawn();
        }
    }

    public virtual IEnumerator DespawnAfterTimeLife(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        NetworkObject.Despawn();
    }

    public void OnPickedUp(GameObject picker)
    {
        throw new System.NotImplementedException();
    }
}
