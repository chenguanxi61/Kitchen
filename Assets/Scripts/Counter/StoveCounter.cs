using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter
{
    private enum State
    {
        Idle,
        Frying,
        Fryed,
        Burned,
    }
    [SerializeField] private FryRecipeSO[] fryRecipeSOArray;
    private float fryingTimer;
    private float burningTime;
    private State state;
    private FryRecipeSO fryRecipeSO;
    
    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObj())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Frying:
                    fryingTimer += Time.deltaTime;
                    if (fryingTimer > fryRecipeSO.fryTimerMax)
                    {
                        
                        GetKitchenObj().DestroySelf();
                        KitchenObj.SpawnKitchenObj(fryRecipeSO.output, this);
                        state = State.Fryed;
                    }
                    break;
                case State.Fryed:
                    burningTime += Time.deltaTime;
                    if (burningTime > fryRecipeSO.fryTimerMax)
                    {
                        
                        GetKitchenObj().DestroySelf();
                        KitchenObj.SpawnKitchenObj(fryRecipeSO.output, this);
                        state = State.Fryed;
                    }
                    break;
                case State.Burned:
                    break;
            }
        }
        
    }
    

   

    public override void Interact(Player player)
    {
        // 灶空：接收可烹饪食材
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj())
            {
                KitchenObjSO input = player.GetKitchenObj().GetKitchenObjSO();
                if (GetFryRecipeSOWithInput(input) != null)   // 一次查找
                {
                    player.GetKitchenObj().SetKitchenObjParent(this);
                    
                    fryRecipeSO= GetFryRecipeSOWithInput(GetKitchenObj().GetKitchenObjSO());
                    
                    state = State.Frying;
                    fryingTimer = 0;
                }
            }
            return;
        }

        // 情况 2：柜台有东西 → 仅当玩家空手时才递给他
        if (!player.HasKitchenObj())
        {
            GetKitchenObj().SetKitchenObjParent(player);
         
        }
    }


    private bool HasFryRecipeWithInput(KitchenObjSO input)
    {
        FryRecipeSO fryRecipeSO = GetFryRecipeSOWithInput(input);
        return fryRecipeSO != null;
        
    }

    private KitchenObjSO GetOutputForInput(KitchenObjSO input)
    {
        FryRecipeSO fryRecipeSO = GetFryRecipeSOWithInput(input);
        if(fryRecipeSO != null)
        {
            return fryRecipeSO.output;
        }
        return null;
    }
    private FryRecipeSO GetFryRecipeSOWithInput(KitchenObjSO input)
    {
        foreach(FryRecipeSO fryRecipeSO in fryRecipeSOArray)
        {
            if(fryRecipeSO.input == input)
            {
                return fryRecipeSO;
            }
        }
        return null;
    }
}
