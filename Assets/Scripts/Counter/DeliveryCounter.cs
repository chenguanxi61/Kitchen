using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    public override void Interact(Player player)
    {
        if (player.HasKitchenObj())
        {
            if(player.GetKitchenObj().TryGetPlate(out PlateKitchObj plateKitchObj))
            {
                DeliverManager.Instance.DeliverRecipe(plateKitchObj);
                plateKitchObj.DestroySelf();
             }
        }
    }
}
