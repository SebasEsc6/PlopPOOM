using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PickableBase : NetworkBehaviour, IPickable
{
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb2D;

    public float limitFallValue;
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
    void FixedUpdate()
    {
        if (IsGrounded(0.5f, LayerMask.GetMask("Ground")))
        {
            StopFall();
        }
        if (transform.position.y < limitFallValue)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic;
            NetworkObject.Despawn();
        }
    }

    public void StopFall()
    {
        rb2D.bodyType = RigidbodyType2D.Static;
    }

    public bool IsGrounded(float distance, LayerMask groundLayer)
    {
        // origin point
        Vector2 leftOrigin = new Vector2(transform.position.x - 0.25f, transform.position.y);
        Vector2 rightOrigin = new Vector2(transform.position.x + 0.25f, transform.position.y);

        // down raycast from two sites
        RaycastHit2D hitLeft = Physics2D.Raycast(leftOrigin, Vector2.down, distance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightOrigin, Vector2.down, distance, groundLayer);

        // visaul debug
        Debug.DrawRay(leftOrigin, Vector2.down * distance, Color.red);
        Debug.DrawRay(rightOrigin, Vector2.down * distance, Color.blue);

        // return true if one of the raycast is true
        return hitLeft.collider != null || hitRight.collider != null;
    }

    public virtual IEnumerator DespawnAfterTimeLife(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        NetworkObject.Despawn();
    }

    public void OnPickedUp(GameObject picker)
    {
        throw new System.NotImplementedException();
    }
}
