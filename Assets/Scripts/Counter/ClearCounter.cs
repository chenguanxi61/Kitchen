using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter 
{
    [SerializeField] private KitchenObjSO kitchenObjSO;
    
    
   

    public override void Interact(Player player)
   {
       if (!HasKitchenObj())
       {
           //没东西
           if (player.HasKitchenObj())
           {
               //放东西
               player.GetKitchenObj().SetKitchenObjParent(this);
              
           }
           else
           {
               
           }
       }
       else
       {
           if (player.HasKitchenObj())
           {
               //玩家拿着东西
           }
           else
           {
               //玩家没拿着东西
               GetKitchenObj().SetKitchenObjParent(player);
           }
       }
   }
    
    
}
