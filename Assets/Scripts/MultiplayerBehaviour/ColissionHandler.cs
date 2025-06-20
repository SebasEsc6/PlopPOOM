using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Receives collision RPCs and forwards them to cached responders.
/// </summary>
[DisallowMultipleComponent]
public class CollisionHandler : NetworkBehaviour, ICollisionHandler
{
    private ICollisionResponder responder;

    public override void OnNetworkSpawn()
    {
        responder = GetComponent<ICollisionResponder>();
        if (responder == null)
            Debug.LogWarning("[CollisionHandler] No ICollisionResponder found on object.");
    }

    public void SendCollisionMessage(CollisionMessageInfo msg)
    {
        HandleCollisionRpc(msg);
    }

    [Rpc(SendTo.Authority, DeferLocal = true)]
    public void HandleCollisionRpc(CollisionMessageInfo msg, RpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        responder?.OnCollision(msg);
    }
}
