using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

using Random = UnityEngine.Random;

public class DeliverManager : NetworkBehaviour
{
    public static DeliverManager Instance { get; private set; }

    public Action OnRecipeSpawned;
    public Action OnRecipeCompleted;
    public Action OnRecipeExpired;
    public Action OnDeliverySuccess;
    public Action OnDeliveryFail;
    public Action OnScoreChanged;

    [SerializeField] private List<RecipeSO> recipeSOList;
    [SerializeField] private int waitingRecipeMax = 4;

    private readonly List<DeliveryOrder> waitingOrderList = new List<DeliveryOrder>();
    private int successfulDeliveries;
    private int score;
    private Coroutine spawnCoroutine;
    private bool hasGameStarted;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying())
        {
            return;
        }

        if (IsServer)
        {
            StartOrders();
        }
        else
        {
            hasGameStarted = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private void Update()
    {
        if (!hasGameStarted)
        {
            return;
        }

        TickOrders(Time.deltaTime, IsServer);
    }

    private void GameManager_OnStateChanged(GameManager.State state)
    {
        if (state != GameManager.State.GamePlaying)
        {
            return;
        }

        if (IsServer)
        {
            StartOrders();
        }
        else
        {
            hasGameStarted = true;
        }
    }

    private void StartOrders()
    {
        if (hasGameStarted)
        {
            return;
        }

        hasGameStarted = true;
        SpawnNewRecipe();
        spawnCoroutine = StartCoroutine(SpawnRecipeLoop());
    }

    private void TickOrders(float deltaTime, bool expireTimedOutOrders)
    {
        for (int i = waitingOrderList.Count - 1; i >= 0; i--)
        {
            waitingOrderList[i].Tick(deltaTime);

            if (expireTimedOutOrders && waitingOrderList[i].TimeRemaining <= 0f)
            {
                ExpireOrder(i);
            }
        }
    }

    private IEnumerator SpawnRecipeLoop()
    {
        while (true)
        {
            if (waitingOrderList.Count < waitingRecipeMax)
            {
                yield return new WaitForSeconds(4f);
                SpawnNewRecipe();
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnNewRecipe()
    {
        if (!IsServer || waitingOrderList.Count >= waitingRecipeMax)
        {
            return;
        }

        int randomIndex = Random.Range(0, recipeSOList.Count);
        AddRecipe(randomIndex);
        SpawnNewRecipeClientRpc(randomIndex);
    }

    [ClientRpc]
    private void SpawnNewRecipeClientRpc(int recipeIndex)
    {
        if (IsServer)
        {
            return;
        }

        hasGameStarted = true;
        AddRecipe(recipeIndex);
    }

    private void AddRecipe(int recipeIndex)
    {
        RecipeSO recipe = recipeSOList[recipeIndex];
        waitingOrderList.Add(new DeliveryOrder(recipe));
        Debug.Log("Adding recipe: " + recipeSOList.Count.ToString());
        OnRecipeSpawned?.Invoke();
    }

    public void DeliverRecipe(PlateKitchObj plateKitchObj)
    {
        DeliverRecipeServerRpc(plateKitchObj.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverRecipeServerRpc(NetworkObjectReference plateNetworkObjectReference)
    {
        if (!plateNetworkObjectReference.TryGet(out NetworkObject plateNetworkObject))
        {
            return;
        }

        PlateKitchObj plateKitchObj = plateNetworkObject.GetComponent<PlateKitchObj>();
        if (plateKitchObj == null)
        {
            return;
        }

        int matchingRecipeIndex = GetMatchingRecipeIndex(plateKitchObj.GetKitchenObjSOList());
        if (matchingRecipeIndex == -1)
        {
            Debug.Log("Delivery failed.");
            OnDeliveryFail?.Invoke();
            DeliverIncorrectRecipeClientRpc();
            return;
        }

        int earnedScore = waitingOrderList[matchingRecipeIndex].GetCurrentScore();
        KitchenObj.DestoryKitchenObj(plateKitchObj);
        DeliverCorrectRecipe(matchingRecipeIndex, earnedScore);
        DeliverCorrectRecipeClientRpc(matchingRecipeIndex, earnedScore);
    }

    private int GetMatchingRecipeIndex(List<KitchenObjSO> plateList)
    {
        for (int i = 0; i < waitingOrderList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingOrderList[i].Recipe;

            if (waitingRecipeSO.kitchenObjSOList.Count != plateList.Count)
            {
                continue;
            }

            bool plateMatchesRecipe = true;
            foreach (KitchenObjSO recipeKitchenObjSO in waitingRecipeSO.kitchenObjSOList)
            {
                bool ingredientFound = false;
                foreach (KitchenObjSO plateKitchenObjSO in plateList)
                {
                    if (plateKitchenObjSO == recipeKitchenObjSO)
                    {
                        ingredientFound = true;
                        break;
                    }
                }

                if (!ingredientFound)
                {
                    plateMatchesRecipe = false;
                    break;
                }
            }

            if (plateMatchesRecipe)
            {
                return i;
            }
        }

        return -1;
    }

    private void ExpireOrder(int orderIndex)
    {
        if (orderIndex < 0 || orderIndex >= waitingOrderList.Count)
        {
            return;
        }

        waitingOrderList.RemoveAt(orderIndex);
        OnRecipeCompleted?.Invoke();
        OnRecipeExpired?.Invoke();
        OnDeliveryFail?.Invoke();
        ExpireOrderClientRpc(orderIndex);
    }

    [ClientRpc]
    private void ExpireOrderClientRpc(int orderIndex)
    {
        if (IsServer || orderIndex < 0 || orderIndex >= waitingOrderList.Count)
        {
            return;
        }

        waitingOrderList.RemoveAt(orderIndex);
        OnRecipeCompleted?.Invoke();
        OnRecipeExpired?.Invoke();
        OnDeliveryFail?.Invoke();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        if (IsServer)
        {
            return;
        }

        OnDeliveryFail?.Invoke();
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int recipeIndex, int earnedScore, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            return;
        }

        DeliverCorrectRecipe(recipeIndex, earnedScore);
    }

    private void DeliverCorrectRecipe(int recipeIndex, int earnedScore)
    {
        if (recipeIndex < 0 || recipeIndex >= waitingOrderList.Count)
        {
            return;
        }

        Debug.Log("Delivery success.");
        successfulDeliveries++;
        score += earnedScore;
        waitingOrderList.RemoveAt(recipeIndex);
        OnRecipeCompleted?.Invoke();
        OnDeliverySuccess?.Invoke();
        OnScoreChanged?.Invoke();
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        List<RecipeSO> recipeList = new List<RecipeSO>();
        foreach (DeliveryOrder order in waitingOrderList)
        {
            recipeList.Add(order.Recipe);
        }

        return recipeList;
    }

    public List<DeliveryOrder> GetWaitingOrderList()
    {
        return waitingOrderList;
    }

    public int GetSuccessfulDeliveries()
    {
        return successfulDeliveries;
    }

    public int GetScore()
    {
        return score;
    }
}
