using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPLayerGetKitchenObj;
    [SerializeField] private KitchenObjSO kitchenObjSO;
    public override void Interact(Player player)
    {
        if(!player.HasKitchenObj())
        {
            //玩家没拿着东西
            KitchenObj.SpawnKitchenObj(kitchenObjSO,player);
            InteractServerRpc();
        }
        
        
      
    }

    [ServerRpc (RequireOwnership = false)]
    private void InteractServerRpc()
    {
        InteractClientRpc();
    }
    [ClientRpc]
    private void InteractClientRpc()
    {
        OnPLayerGetKitchenObj?.Invoke(this, EventArgs.Empty);
    }
    
}
