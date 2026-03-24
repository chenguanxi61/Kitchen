using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchGameMultiPlayer : NetworkBehaviour
{
    private const int MAX_PLAYERS = 4;
    public static KitchGameMultiPlayer Instance { get; private set; }

    [SerializeField] private KitchenObjListSO kitchenObjListSO;
    [SerializeField] private NetworkObject playerPrefab;

    private Dictionary<ulong, bool> playerReadyDictionary;
    private Dictionary<ulong, NetworkObject> spawnedPlayerDictionary;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
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

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;

        if (!playerReadyDictionary.ContainsKey(senderClientId))
        {
            playerReadyDictionary[senderClientId] = false;
        }

        playerReadyDictionary[senderClientId] = true;
        CheckAllPlayersReady();
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
}
