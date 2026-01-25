using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatesCounter : BaseCounter
{
    public UnityAction OnPlateSpawned;
    public UnityAction OnPlateRemoved;
    
    [SerializeField] private float spawnPlateTimer;
    [SerializeField] private float spawnPlateTimerMax = 4f;
    [SerializeField] private KitchenObjSO plateKitchenObjSO;
    private int plateSpawnedAmount;
    private int plateSpawnedAmountMax = 4;

    private void Update()
    {
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer >= spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;
            if (plateSpawnedAmount < plateSpawnedAmountMax)
            {
                plateSpawnedAmount++;
                OnPlateSpawned?.Invoke();
            }
        }
    }
    
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObj()&&plateSpawnedAmount>0)
        {
            //玩家手上没盘子 拿起盘子
            plateSpawnedAmount--;
            KitchenObj.SpawnKitchenObj(plateKitchenObjSO, player);
            OnPlateRemoved?.Invoke();
        }
    }
}
