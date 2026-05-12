using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchGameMultiPlayer : NetworkBehaviour
{
    public const int MAX_PLAYERS = 4;
    private static readonly Color32[] DEFAULT_PLAYER_COLORS = new Color32[]
    {
        new Color32(244, 67, 54, 255),
        new Color32(33, 150, 243, 255),
        new Color32(76, 175, 80, 255),
        new Color32(255, 193, 7, 255),
        new Color32(156, 39, 176, 255),
        new Color32(255, 152, 0, 255),
    };
    public static KitchGameMultiPlayer Instance { get; private set; }
    public static bool HasLocalClientLeftSession { get; private set; }

    [SerializeField] private KitchenObjListSO kitchenObjListSO;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private List<Color> playerColorList;

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, NetworkObject> spawnedPlayerDictionary;
    private Dictionary<ulong, Color32> playerColorCacheDictionary;
    
    private NetworkList<PlayerData> playerDataNetworkList;//玩家客户端信息
    
    public event Action OnPlayerDataNetWorkListChanged;//玩家列表发生变化事件

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += OnListChange;
        playerColorCacheDictionary = new Dictionary<ulong, Color32>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnListChange(NetworkListEvent<PlayerData> changeEvent)
    {
        SyncPlayerColorCacheFromNetworkList();
        OnPlayerDataNetWorkListChanged?.Invoke();
    }

    public void StartHost()
    {
        HasLocalClientLeftSession = false;
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.StartHost();
        RefreshPlayerDataListFromConnectedClients();
    }
    //有客户端链接进来就更新 
    private void Singleton_OnClientConnectedCallback(ulong clientID)
    {
        AddPlayerDataIfMissing(clientID);
    }

    public bool StartClient()
    {
        if (HasLocalClientLeftSession)
        {
            Debug.LogWarning("This client already left the session and cannot reconnect in this run.");
            return false;
        }

        return NetworkManager.Singleton.StartClient();
    }

    public static void MarkLocalClientLeftSession()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            HasLocalClientLeftSession = true;
        }
    }

    public void SpawnKitchenObj(KitchenObjSO kitchenObjSO, IKitchObjParent kitchenObjParent)
    {
        SpawnKitchenObjServerRpc(GetKitchenObjIndex(kitchenObjSO), kitchenObjParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjServerRpc(int kitchenObjSOIndex, NetworkObjectReference kitchenObjParentNetworkReference)
    {
        KitchenObjSO kitchenObjSO = GetKitchenObjByIndex(kitchenObjSOIndex);
        Transform kitchenObjTransform = Instantiate(kitchenObjSO.prefab);
        NetworkObject networkObject = kitchenObjTransform.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        if (!kitchenObjParentNetworkReference.TryGet(out NetworkObject kitchenObjParentNetworkObj))
        {
            return;
        }

        IKitchObjParent kitchenObjParent = kitchenObjParentNetworkObj.GetComponent<IKitchObjParent>();
        kitchenObjTransform.GetComponent<KitchenObj>().SetKitchenObjParent(kitchenObjParent);
    }

    public int GetKitchenObjIndex(KitchenObjSO kitchenObjSO)
    {
        return kitchenObjListSO.kitchenObjSOList.IndexOf(kitchenObjSO);
    }

    public KitchenObjSO GetKitchenObjByIndex(int kitchenObjIndex)
    {
        return kitchenObjListSO.kitchenObjSOList[kitchenObjIndex];
    }

    public void DestroyKitchenObj(KitchenObj kitchenObj)
    {
        DestoryKitchenObjServerRpc(kitchenObj.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestoryKitchenObjServerRpc(NetworkObjectReference kitchenObjReference)
    {
        if (!kitchenObjReference.TryGet(out NetworkObject kitchenObjNetworkObject))
        {
            return;
        }

        KitchenObj kitchenObj = kitchenObjNetworkObject.GetComponent<KitchenObj>();
        ClearKitchenObjOnParentClientRpc(kitchenObjReference);

        if (kitchenObj != null)
        {
            kitchenObj.ClearKitchenObjOnParent();
        }

        if (kitchenObjNetworkObject.IsSpawned)
        {
            kitchenObjNetworkObject.Despawn(true);
        }
        else
        {
            Destroy(kitchenObjNetworkObject.gameObject);
        }
    }

    [ClientRpc]
    private void ClearKitchenObjOnParentClientRpc(NetworkObjectReference kitchenObjReference)
    {
        if (!kitchenObjReference.TryGet(out NetworkObject kitchenObjNetworkObject))
        {
            return;
        }

        KitchenObj kitchenObj = kitchenObjNetworkObject.GetComponent<KitchenObj>();

        if (kitchenObj != null)
        {
            kitchenObj.ClearKitchenObjOnParent();
        }
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    public void SetPlayerColor(Color color)
    {
        Color32 color32 = color;
        PreviewLocalPlayerColor(color32);
        SetPlayerColorServerRpc(color32.r, color32.g, color32.b);
    }

    public void SetPlayerColorByIndex(int colorIndex)
    {
        Color32 color = GetPlayerColor(colorIndex);
        PreviewLocalPlayerColor(color);
        SetPlayerColorServerRpc(color.r, color.g, color.b);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        if (!playerReadyDictionary.ContainsKey(senderClientId))
        {
            playerReadyDictionary[senderClientId] = false;
        }

        playerReadyDictionary[senderClientId] = true;
        SetPlayerDataReady(senderClientId, true);
        CheckAllPlayersReady();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerColorServerRpc(byte r, byte g, byte b, ServerRpcParams serverRpcParams = default)
    {
        // 通过 ServerRpc 拿到实际发起颜色修改请求的客户端。
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        // 颜色由服务器统一判重，避免多个客户端本地判断不一致。
        if (IsColorUsedByOtherClient(r, g, b, senderClientId))
        {
            return;
        }

        // 修改服务器上的玩家颜色数据，后续会通过同步逻辑刷新所有客户端显示。
        SetPlayerDataColor(senderClientId, r, g, b);
    }

    

    private void CheckAllPlayersReady()
    {
        if (!IsServer || playerReadyDictionary.Count == 0)
        {
            return;
        }

        foreach (bool isReady in playerReadyDictionary.Values)
        {
            if (!isReady)
            {
                return;
            }
        }

        Loader.LoadNetwork(Loader.Scene.GameScene);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        if (playerReadyDictionary == null || !playerReadyDictionary.ContainsKey(clientId))
        {
            return false;
        }

        return playerReadyDictionary[clientId];
    }

    public List<ulong> GetConnectedClientIdList()
    {
        if (playerReadyDictionary == null)
        {
            return new List<ulong>();
        }

        return new List<ulong>(playerReadyDictionary.Keys);
    }

    public override void OnNetworkSpawn()
    {
        playerReadyDictionary = new Dictionary<ulong, bool>();
        spawnedPlayerDictionary = new Dictionary<ulong, NetworkObject>();
        playerColorCacheDictionary = new Dictionary<ulong, Color32>();

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

        if (!IsServer)
        {
            return;
        }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId))
            {
                playerReadyDictionary[clientId] = false;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneManager_OnLoadEventCompleted;
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }

        playerReadyDictionary[clientId] = false;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (!IsServer)
        {
            return;
        }

        if (playerReadyDictionary.ContainsKey(clientId))
        {
            playerReadyDictionary.Remove(clientId);
        }

        if (spawnedPlayerDictionary.ContainsKey(clientId))
        {
            spawnedPlayerDictionary.Remove(clientId);
        }

        RemovePlayerDataIfExists(clientId);
    }

    private void SceneManager_OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer || sceneName != Loader.Scene.GameScene.ToString())
        {
            return;
        }

        SpawnPlayersForAllClients();
    }

    private void SpawnPlayersForAllClients()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient networkClient) &&
                networkClient.PlayerObject != null)
            {
                continue;
            }

            if (spawnedPlayerDictionary.ContainsKey(clientId))
            {
                continue;
            }

            NetworkObject playerNetworkObject = Instantiate(playerPrefab);
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
            spawnedPlayerDictionary[clientId] = playerNetworkObject;
        }
    }
    //判断该索引玩家是否链接
    public bool IsPlayerIndexConnected(int playerIndex)
    {
        return playerIndex < playerDataNetworkList.Count;
    }

    public bool IsPlayerIndexReady(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerDataNetworkList.Count)
        {
            return false;
        }

        return playerDataNetworkList[playerIndex].isReady;
    }

    public Color GetPlayerColorByIndex(int playerIndex)
    {
        if (!TryGetPlayerClientIdByIndex(playerIndex, out ulong clientId))
        {
            return Color.white;
        }

        return GetPlayerColor(clientId);
    }

    public Color GetPlayerColor(ulong clientId)
    {
        if (playerColorCacheDictionary != null && playerColorCacheDictionary.TryGetValue(clientId, out Color32 cachedColor))
        {
            return cachedColor;
        }

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId)
            {
                return new Color32(playerData.colorR, playerData.colorG, playerData.colorB, 255);
            }
        }

        return Color.white;
    }

    public bool TryGetPlayerClientIdByIndex(int playerIndex, out ulong clientId)
    {
        clientId = 0;
        if (playerIndex < 0 || playerIndex >= playerDataNetworkList.Count)
        {
            return false;
        }

        clientId = playerDataNetworkList[playerIndex].clientId;
        return true;
    }

    public bool TryGetPlayerIndexByClientId(ulong clientId, out int playerIndex)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                playerIndex = i;
                return true;
            }
        }

        playerIndex = -1;
        return false;
    }

    public bool TryGetLocalPlayerIndex(out int playerIndex)
    {
        playerIndex = -1;

        if (NetworkManager.Singleton == null)
        {
            return false;
        }

        return TryGetPlayerIndexByClientId(NetworkManager.Singleton.LocalClientId, out playerIndex);
    }

    public bool IsColorIndexAvailableForClient(int colorIndex, ulong clientId)
    {
        Color32 color = GetPlayerColor(colorIndex);
        return !IsColorUsedByOtherClient(color.r, color.g, color.b, clientId);
    }

    private void RefreshPlayerDataListFromConnectedClients()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        playerDataNetworkList.Clear();
        foreach (ulong connectedClientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            AddPlayerDataIfMissing(connectedClientId);
        }
    }

    private void AddPlayerDataIfMissing(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (PlayerData playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId)
            {
                return;
            }
        }

        Color32 defaultColor = GetPlayerColor(playerDataNetworkList.Count);
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            isReady = false,
            colorR = defaultColor.r,
            colorG = defaultColor.g,
            colorB = defaultColor.b
        });
        playerColorCacheDictionary[clientId] = defaultColor;
    }

    private void RemovePlayerDataIfExists(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                playerDataNetworkList.RemoveAt(i);
                if (playerColorCacheDictionary.ContainsKey(clientId))
                {
                    playerColorCacheDictionary.Remove(clientId);
                }
                return;
            }
        }
    }

    private void SetPlayerDataReady(ulong clientId, bool isReady)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                PlayerData playerData = playerDataNetworkList[i];
                if (playerData.isReady == isReady)
                {
                    return;
                }

                playerData.isReady = isReady;
                playerDataNetworkList[i] = playerData;
                return;
            }
        }
    }

    public int GetPlayerColorCount()
    {
        if (playerColorList != null && playerColorList.Count > 0)
        {
            return playerColorList.Count;
        }

        return DEFAULT_PLAYER_COLORS.Length;
    }

    public Color GetPlayerColor(int colorId)
    {
        if (playerColorList != null && playerColorList.Count > 0)
        {
            int clampedColorId = Mathf.Clamp(colorId, 0, playerColorList.Count - 1);
            return playerColorList[clampedColorId];
        }

        int fallbackIndex = Mathf.Abs(colorId) % DEFAULT_PLAYER_COLORS.Length;
        return DEFAULT_PLAYER_COLORS[fallbackIndex];
    }

    private void PreviewLocalPlayerColor(Color32 color)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        playerColorCacheDictionary[localClientId] = color;
        OnPlayerDataNetWorkListChanged?.Invoke();
    }

    private void SetPlayerDataColor(ulong clientId, byte r, byte g, byte b)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            if (playerDataNetworkList[i].clientId == clientId)
            {
                PlayerData playerData = playerDataNetworkList[i];
                if (playerData.colorR == r && playerData.colorG == g && playerData.colorB == b)
                {
                    return;
                }

                playerData.colorR = r;
                playerData.colorG = g;
                playerData.colorB = b;
                playerDataNetworkList[i] = playerData;
                playerColorCacheDictionary[clientId] = new Color32(r, g, b, 255);
                SyncPlayerColorClientRpc(clientId, r, g, b);
                return;
            }
        }
    }

    [ClientRpc]
    private void SyncPlayerColorClientRpc(ulong clientId, byte r, byte g, byte b)
    {
        playerColorCacheDictionary[clientId] = new Color32(r, g, b, 255);
        OnPlayerDataNetWorkListChanged?.Invoke();
    }

    private bool IsColorUsedByOtherClient(byte r, byte g, byte b, ulong excludeClientId)
    {
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == excludeClientId)
            {
                continue;
            }

            if (playerData.colorR == r && playerData.colorG == g && playerData.colorB == b)
            {
                return true;
            }
        }

        return false;
    }

    private void SyncPlayerColorCacheFromNetworkList()
    {
        if (playerColorCacheDictionary == null)
        {
            playerColorCacheDictionary = new Dictionary<ulong, Color32>();
        }

        List<ulong> staleClientIdList = new List<ulong>(playerColorCacheDictionary.Keys);
        for (int i = 0; i < playerDataNetworkList.Count; i++)
        {
            PlayerData playerData = playerDataNetworkList[i];
            playerColorCacheDictionary[playerData.clientId] =
                new Color32(playerData.colorR, playerData.colorG, playerData.colorB, 255);
            staleClientIdList.Remove(playerData.clientId);
        }

        foreach (ulong staleClientId in staleClientIdList)
        {
            playerColorCacheDictionary.Remove(staleClientId);
        }
    }
}
