using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class DeliverManager : MonoBehaviour
{
    public static DeliverManager Instance { get; private set; }
    
    public Action OnRecipeSpawned; 
    public Action OnRecipeCompleted; 
    
    public Action OnDeliverySuccess;
    public Action OnDeliveryFail;
    
    private List<RecipeSO> waitingRecipeSOList;   // 等待中菜品
    [SerializeField] private List<RecipeSO> recipeSOList; // 所有菜品
    [SerializeField] private int waitingRecipeMax = 4;

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Start()
    {
        // 开局立刻生成一次
        SpawnNewRecipe();

        // 启动循环生成
        spawnCoroutine = StartCoroutine(SpawnRecipeLoop());
    }

    private IEnumerator SpawnRecipeLoop()
    {
        while (true)
        {
            // 如果没满，才等待并生成
            if (waitingRecipeSOList.Count < waitingRecipeMax)
            {
                yield return new WaitForSeconds(4f);
                SpawnNewRecipe();
            }
            else
            {
                // 满了就暂停一帧，避免空转
                yield return null;
            }
        }
    }

    private void SpawnNewRecipe()
    {
        if (waitingRecipeSOList.Count >= waitingRecipeMax) return;

        int randomIndex = Random.Range(0, recipeSOList.Count);
        RecipeSO randomRecipeSO = recipeSOList[randomIndex];
        waitingRecipeSOList.Add(randomRecipeSO);
        Debug.Log("New Recipe Spawned: " + randomRecipeSO.name);
        // TODO: 触发 UI 更新事件
        OnRecipeSpawned?.Invoke();
    }
    //送餐
    public void DeliverRecipe(PlateKitchObj plateKitchObj)
    {
        List<KitchenObjSO> plateList = plateKitchObj.GetKitchenObjSOList();// 盘子里的食材列表

        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            // 1. 数量不一致，直接跳过
            if (waitingRecipeSO.kitchenObjSOList.Count != plateList.Count)
            {
                continue;
            }

            bool plateMatchesRecipe = true;// 盘子和菜谱是否匹配

            // 2. 检查菜谱里的每一种食材
            foreach (KitchenObjSO recipeKitchenObjSO in waitingRecipeSO.kitchenObjSOList)
            {
                bool ingredientFound = false;

                // 3. 看盘子里有没有这个食材
                foreach (KitchenObjSO plateKitchenObjSO in plateList)
                {
                    if (plateKitchenObjSO == recipeKitchenObjSO)
                    {
                        ingredientFound = true;
                        break;
                    }
                }

                // 4. 有一个没找到，直接失败
                if (!ingredientFound)
                {
                    plateMatchesRecipe = false;
                    
                    break;
                }
            }

            // 5. 全部匹配成功
            if (plateMatchesRecipe)
            {
                Debug.Log("送菜成功！");

                waitingRecipeSOList.RemoveAt(i);

                // TODO：加分 / UI / 音效 / 事件
                OnRecipeCompleted?.Invoke();
                OnDeliverySuccess?.Invoke();
                return;
            }
        }

        Debug.Log("送菜失败！");
        OnDeliveryFail?.Invoke();
    }
    
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }
}