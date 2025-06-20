using Unity.Netcode;

public interface ICollisionHandler
{
    /// <summary>
    /// Non-authority instances call this to forward a collision.
    /// </summary>
    void SendCollisionMessage(CollisionMessageInfo collisionMessage);

    /// <summary>
    /// Authority instances receive forwarded collisions here.
    /// </summary>
    [Rpc(SendTo.Authority, DeferLocal = true)]
    void HandleCollisionRpc(CollisionMessageInfo collisionMessage, RpcParams rpcParams = default);
}