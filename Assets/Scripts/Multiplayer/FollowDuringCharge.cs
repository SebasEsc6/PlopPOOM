using UnityEngine;
using Unity.Netcode;

public class FollowDuringCharge : NetworkBehaviour
{
    Transform target;
    public void Init(Transform t) => target = t;

    void Update()
    {
        if (!HasAuthority) return;                 
        if (target != null) transform.position = target.position;
    }
}

