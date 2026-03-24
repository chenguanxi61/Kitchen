using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static Action<Vector3> OnAnyObjectTrashed;
    public override void Interact(Player player)
    {
        if(player.HasKitchenObj())
        {
            KitchenObj.DestoryKitchenObj(player.GetKitchenObj());
            
            InteractServerRpc();
            
        }
    }
    [ServerRpc  (RequireOwnership = false)]
    private void InteractServerRpc()
    {
        InteractClientRpc();
    }
    [ClientRpc]
    private void InteractClientRpc()
    {
        OnAnyObjectTrashed?.Invoke(transform.position);
    }
}
