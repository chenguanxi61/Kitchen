using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class CuttingCounter : BaseCounter, IHasProgressBar
{
   [SerializeField] private CuttingRecipeSO[] cuttingRecipesSOArray;
   private IHasProgressBar hasProgressBar;
   
   private int cuttingProgress;
   
   public event UnityAction<float> OnProgressChanged;
   
   
   public override void Interact(Player player)
   {
      // 情况 1：柜台空 → 尝试把玩家手里的东西放上来（必须有配方才收）
      if (!HasKitchenObj())
      {
         if (player.HasKitchenObj() && HasRecipeWithInput(player.GetKitchenObj().GetKitchenObjSO()))
         {
            player.GetKitchenObj().SetKitchenObjParent(this);
            cuttingProgress = 0;
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSO(GetKitchenObj().GetKitchenObjSO());
            OnProgressChanged?.Invoke((float)cuttingProgress/cuttingRecipeSO.cuttingProgressMax);
         }
         return;               // 无论如何都结束
      }

      // 情况 2：柜台有东西 → 仅当玩家空手时才递给他
      if (!player.HasKitchenObj())
      {
         GetKitchenObj().SetKitchenObjParent(player);
         
      }
   }
   public override void InteractAlternate(Player player)
   {
      if (HasKitchenObj()&&HasRecipeWithInput(GetKitchenObj().GetKitchenObjSO()))
      {
         //有东西 切菜
         cuttingProgress++;
         CuttingRecipeSO recipe = GetCuttingRecipeSO(GetKitchenObj().GetKitchenObjSO());
         OnProgressChanged?.Invoke((float)cuttingProgress/recipe.cuttingProgressMax);
        
         if (cuttingProgress >= recipe.cuttingProgressMax)
         {
            KitchenObjSO outputKitchenObj = GetCuttingObjSO(GetKitchenObj().GetKitchenObjSO());
            GetKitchenObj().DestroySelf();
            KitchenObj.SpawnKitchenObj(outputKitchenObj,this);
         }
         
         
      }
   }

   public bool HasRecipeWithInput(KitchenObjSO inputKitchenObjSO)
   {
      CuttingRecipeSO recipe = GetCuttingRecipeSO(inputKitchenObjSO);
      return recipe != null;
   }

   private KitchenObjSO GetCuttingObjSO(KitchenObjSO input)
   {
      CuttingRecipeSO recipe = GetCuttingRecipeSO(input);
      if (recipe != null)
      {
         return recipe.output;
      }
      return null;
   }
   //obj厨房物品 recipe 切好的
   private CuttingRecipeSO GetCuttingRecipeSO(KitchenObjSO input)
   {
      foreach (CuttingRecipeSO recipeSO in cuttingRecipesSOArray)
      {
         if (recipeSO.input == input)
         {
            return recipeSO;
         }
      }
      return null;
   }
}
