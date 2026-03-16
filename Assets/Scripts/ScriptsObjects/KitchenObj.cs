using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchenObj : NetworkBehaviour
{
    [SerializeField] private KitchenObjSO kitchenObjSO;
    //判断谁拿到了自己
    private IKitchObjParent iKitchObjParent;
    
    //转换
    private FollowTransfrom followTransfrom;


    protected virtual void Awake()
    {
        followTransfrom = GetComponent<FollowTransfrom>();
    }

    public KitchenObjSO GetKitchenObjSO()
    {
        return kitchenObjSO;
    }
    
    public void SetKitchenObjParent(IKitchObjParent newKitchenObjParent)
    {
       SetKitchenObjParentServerRpc(newKitchenObjParent.GetNetworkObject());
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjParentServerRpc(NetworkObjectReference kitchenObjParentNetworkReference)
    {
        SetKitchenObjParentClientRpc(kitchenObjParentNetworkReference);
    }
    [ClientRpc]
    private void SetKitchenObjParentClientRpc(NetworkObjectReference kitchenObjParentNetworkReference)
    {
        //拿到父物体
        kitchenObjParentNetworkReference.TryGet(out NetworkObject kitchenObjParentNetworkObj);
        IKitchObjParent newKitchenObjParent = kitchenObjParentNetworkObj.GetComponent<IKitchObjParent>();
        // 1. 清理旧桌台
        if (iKitchObjParent != null)
        {
            iKitchObjParent.ClearKitchenObj();
        }

        // 2. 切换到新桌台
        iKitchObjParent = newKitchenObjParent;

        // 3. 新桌台不应当有 KitchenObj
        if (newKitchenObjParent.HasKitchenObj())
        {
            Debug.LogError("新的父物体已经有物品了！");
        }

        // 4. 给新桌台赋值
        newKitchenObjParent.SetKitchenObj(this);

        // 5. 设置物体位置
        followTransfrom.SetTargetTransfrom(newKitchenObjParent.GetTopPoint());

    }

    public IKitchObjParent GetKitchenObjParent()
    {
        return iKitchObjParent;
    }
    
    
    public void DestroySelf()
    {
        // 1. 清理旧桌台
        if (iKitchObjParent != null)
        {
            iKitchObjParent.ClearKitchenObj();
        }
        // 2. 销毁物体
        Destroy(gameObject);
    }
    
    public static void SpawnKitchenObj(KitchenObjSO kitchenObjSO,IKitchObjParent kitchenObjParent)
    {
        KitchGameMultiPlayer.Instance.SpawnKitchenObj(kitchenObjSO,kitchenObjParent);
        
    }
    
    public bool TryGetPlate(out PlateKitchObj plateKitchObj)
    {
        if (this is PlateKitchObj)
        {
            plateKitchObj = this as PlateKitchObj;
            return true;
        }
        else
        {
            plateKitchObj = null;
            return false;
        }
    }
}
