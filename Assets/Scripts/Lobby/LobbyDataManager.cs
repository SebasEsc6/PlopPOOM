using System;
using Unity.Netcode;

public class LobbyDataManager : NetworkBehaviour
{
    public static LobbyDataManager Instance { get; private set; }

    public NetworkVariable<int> SelectedMap = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public struct PlayerInfo : INetworkSerializable, IEquatable<PlayerInfo>
    {
        public ulong ClientId;
        public int PrefabHash;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PrefabHash);
        }

        public bool Equals(PlayerInfo other) =>
            ClientId == other.ClientId && PrefabHash == other.PrefabHash;
    }

    public NetworkList<PlayerInfo> players = new NetworkList<PlayerInfo>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        players.Add(new PlayerInfo { ClientId = clientId, PrefabHash = -1 });
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPrefabHashServerRpc(int hash, ServerRpcParams rpc = default)
    {
        ulong sender = rpc.Receive.SenderClientId;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == sender)
            {
                var info = players[i];
                info.PrefabHash = hash;
                players[i] = info;
                break;
            }
        }
    }

    public void ChooseMap(int mapIndex)
    {
        if (!IsServer) return;
        SelectedMap.Value = mapIndex;
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        base.OnDestroy();
    }
}
