using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlateKitchObj : KitchenObj
{
    [SerializeField] private List<KitchenObjSO> validKitchenObjSOList;//  盘子里可以装的东西列表
    private List<KitchenObjSO> plateKitchenObjSOList;
    public Action<KitchenObjSO> OnAddSomething;

    public void Awake()
    {
        plateKitchenObjSOList = new List<KitchenObjSO>();
    }
    //尝试往盘子是装东西
    public bool TryAddSomething(KitchenObjSO kitchenObjSO)
    {
        if(!validKitchenObjSOList.Contains(kitchenObjSO))
        {
            return false;
        }
        if (plateKitchenObjSOList.Contains(kitchenObjSO))
        {
            return false;
        }
        plateKitchenObjSOList.Add(kitchenObjSO);
        OnAddSomething?.Invoke(kitchenObjSO);
        return true; 
    }
}
