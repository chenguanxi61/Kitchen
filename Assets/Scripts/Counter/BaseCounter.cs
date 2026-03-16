using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour,IKitchObjParent
{
    
    [SerializeField] protected GameObject TopPoint;
    //用来判断桌台上是否有物品
    protected KitchenObj kitchenObj;
    
    public static Action<Vector3> OnAnyObjectPlaced; 
    public virtual void Interact(Player player)
    {
        Debug.Log("Interact");
    }
    public virtual void InteractAlternate(Player player)
    {
        Debug.Log("InteractAlternate");
    }
    public Transform GetTopPoint()
    {
        return TopPoint.transform;
    }
    public void SetKitchenObj(KitchenObj kitchenObj)
    {
        this.kitchenObj = kitchenObj;
        if (kitchenObj != null)
        {
            OnAnyObjectPlaced?.Invoke(transform.position);
        }
    }
    //拿到桌面上的东西
    public KitchenObj GetKitchenObj()
    {
        return kitchenObj;
    }
    public void ClearKitchenObj()
    {
        kitchenObj = null;
    }
    public bool HasKitchenObj()
    {
        return kitchenObj != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
