using UnityEngine;

public class PlayerAvatarHandle : MonoBehaviour
{
    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private Transform avatarParent;

    public GameObject CurrentAvatar { get; private set; }

    public void SpawnAvatar(Vector3 pos, Quaternion rot)
    {
        DespawnAvatar();
        CurrentAvatar = PoolManager.Instance.Spawn(avatarPrefab, pos, rot, avatarParent);
    }

    public void DespawnAvatar()
    {
        if (!CurrentAvatar) return;
        PoolManager.Instance.Despawn(CurrentAvatar);
        CurrentAvatar = null;
    }
}

