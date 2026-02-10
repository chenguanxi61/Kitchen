using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static Action<Vector3> OnAnyObjectTrashed;
    public override void Interact(Player player)
    {
        if(player.HasKitchenObj())
        {
            player.GetKitchenObj().DestroySelf();
            OnAnyObjectTrashed?.Invoke(transform.position);
        }
    }
}
