using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter 
{
    [SerializeField] private KitchenObjSO kitchenObjSO;
    
    
   

    public override void Interact(Player player)
   {
       // 情况 1：桌台是空的
       if (!HasKitchenObj())
       {
           if (player.HasKitchenObj())
           {
               // 玩家把物品放到桌台
               player.GetKitchenObj().SetKitchenObjParent(this);
           }
           return;
       }

       // 情况 2：桌台有东西
       if (!player.HasKitchenObj())
       {
           // 玩家空手，直接拿走
           GetKitchenObj().SetKitchenObjParent(player);
           return;
       }

       // 情况 3：双方都有东西 → 尝试合成
       if (player.GetKitchenObj().TryGetPlate(out PlateKitchObj plate))
       {
           if (plate.TryAddSomething(GetKitchenObj().GetKitchenObjSO()))
           {
               GetKitchenObj().DestroySelf();
           }
       }
       //情况4双方都有东西玩家手里不是盘子 桌台上是盘子
       else if (GetKitchenObj().TryGetPlate(out plate))
       {
           if (plate.TryAddSomething(player.GetKitchenObj().GetKitchenObjSO()))
           {
               player.GetKitchenObj().DestroySelf();
           }
       }
       
   }
    
    
}
