using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KitchGameMultiPlayer : NetworkBehaviour
{
   public static KitchGameMultiPlayer Instance{get; private set;}
   //厨房物品列表
   [SerializeField] private KitchenObjListSO kitchenObjListSO;
   private void Awake()
   {
       Instance = this;
   }
   
   public void SpawnKitchenObj(KitchenObjSO kitchenObjSO,IKitchObjParent kitchenObjParent)
   {
       SpawnKitchenObjServerRpc(GetKitchenObjIndex(kitchenObjSO) , kitchenObjParent.GetNetworkObject());
   }
   
   [ServerRpc (RequireOwnership = false)]
   private void SpawnKitchenObjServerRpc(int kitchenObjSOIndex,NetworkObjectReference kitchenObjParentNetworkReference)
   {
       KitchenObjSO kitchenObjSO = GetKitchenObjByIndex(kitchenObjSOIndex);
       Transform obj = Instantiate(kitchenObjSO.prefab);
       NetworkObject networkObject = obj.transform.GetComponent<NetworkObject>();
       networkObject.Spawn(true);
       
       kitchenObjParentNetworkReference.TryGet(out NetworkObject kitchenObjParentNetworkObj);
       IKitchObjParent kitchObjParent = kitchenObjParentNetworkObj.GetComponent<IKitchObjParent>();
       obj.transform.GetComponent<KitchenObj>().SetKitchenObjParent(kitchObjParent);
   }

   private int GetKitchenObjIndex(KitchenObjSO kitchenObjSO)
   {
       return kitchenObjListSO.kitchenObjSOList.IndexOf(kitchenObjSO);
   }

   private KitchenObjSO GetKitchenObjByIndex(int kitchenObjIndex)
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
       //拿到销毁物体的引用
       kitchenObjReference.TryGet(out NetworkObject kitchenObj);
       KitchenObj kitchenobj1 = kitchenObj.GetComponent<KitchenObj>();
       ClearKitchenObjOnParentClientRpc(kitchenObjReference);
       kitchenobj1.DestroySelf();
   }

    [ClientRpc]
   private void ClearKitchenObjOnParentClientRpc(NetworkObjectReference kitchenObjReference)
   {
       kitchenObjReference.TryGet(out NetworkObject kitchenObj);
       KitchenObj kitchenObj1 = kitchenObj.GetComponent<KitchenObj>();
       kitchenObj1.ClearKitchenObjOnParent();
   }
   
   
}
