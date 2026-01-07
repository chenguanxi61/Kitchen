using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : MonoBehaviour,IKitchObjParent
{
    
    [SerializeField] protected GameObject TopPoint;
    //用来判断桌台上是否有物品
    protected KitchenObj kitchenObj;
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
    }
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
}
