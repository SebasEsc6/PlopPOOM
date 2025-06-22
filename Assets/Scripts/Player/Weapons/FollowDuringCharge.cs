using UnityEngine;
using Unity.Netcode;

public class FollowDuringCharge : NetworkBehaviour
{
    Transform target;
    public void Init(Transform t) => target = t;

    bool CanExecuteClientLogic() => IsSpawned && HasAuthority;

    void Update()
    {
        if (!CanExecuteClientLogic()) return;                 
        if (target != null) transform.position = target.position;
    }
}

