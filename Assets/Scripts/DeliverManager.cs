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
    public Action OnDeliverySuccess;
    public Action OnDeliveryFail;

    [SerializeField] private List<RecipeSO> recipeSOList;
    [SerializeField] private int waitingRecipeMax = 4;

    private readonly List<RecipeSO> waitingRecipeSOList = new List<RecipeSO>();
    private int successfulDeliveries;
    private Coroutine spawnCoroutine;

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
        if (!IsServer)
        {
            return;
        }

        SpawnNewRecipe();
        spawnCoroutine = StartCoroutine(SpawnRecipeLoop());
    }

    public override void OnNetworkDespawn()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRecipeLoop()
    {
        while (true)
        {
            if (waitingRecipeSOList.Count < waitingRecipeMax)
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
        if (!IsServer || waitingRecipeSOList.Count >= waitingRecipeMax)
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

        AddRecipe(recipeIndex);
    }

    private void AddRecipe(int recipeIndex)
    {
        RecipeSO recipe = recipeSOList[recipeIndex];
        waitingRecipeSOList.Add(recipe);
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

        KitchenObj.DestoryKitchenObj(plateKitchObj);
        DeliverCorrectRecipe(matchingRecipeIndex);
        DeliverCorrectRecipeClientRpc(matchingRecipeIndex);
    }

    private int GetMatchingRecipeIndex(List<KitchenObjSO> plateList)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

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
    private void DeliverCorrectRecipeClientRpc(int recipeIndex, ClientRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            return;
        }

        DeliverCorrectRecipe(recipeIndex);
    }

    private void DeliverCorrectRecipe(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= waitingRecipeSOList.Count)
        {
            return;
        }

        Debug.Log("Delivery success.");
        successfulDeliveries++;
        waitingRecipeSOList.RemoveAt(recipeIndex);
        OnRecipeCompleted?.Invoke();
        OnDeliverySuccess?.Invoke();
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulDeliveries()
    {
        return successfulDeliveries;
    }
}
