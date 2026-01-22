using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoveCounter : BaseCounter,IHasProgressBar
{
    public event UnityAction<float> OnProgressChanged; 
    public  UnityAction<State> OnStateChanged;
    //状态机状态
    public enum State
    {
        Idle,
        Frying,
        Fryed,
        Burned,
    }
    [SerializeField] private FryRecipeSO[] fryRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
    private float fryingTimer;//煎的时间
    private FryRecipeSO fryRecipeSO;
    private float burningTime;//烧焦所需的时间
    private BurningRecipeSO burningRecipeSO;
    private State state;
    
    
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
                    OnProgressChanged?.Invoke(fryingTimer / fryRecipeSO.fryTimerMax);
                    if (fryingTimer > fryRecipeSO.fryTimerMax)
                    {
                        //如果当前烹饪时间大于最大烹饪时间，生成煎好的食物
                        GetKitchenObj().DestroySelf();
                        KitchenObj.SpawnKitchenObj(fryRecipeSO.output, this);
                        
                        state = State.Fryed;
                        burningTime = 0f;
                        //拿到台子上物品的烧焦配方SO
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObj().GetKitchenObjSO());
                        
                        OnStateChanged?.Invoke(state);
                        OnProgressChanged?.Invoke(0f);
                        
                    }
                    break;
                case State.Fryed:
                    burningTime += Time.deltaTime;
                    OnProgressChanged?.Invoke(burningTime / burningRecipeSO.burningTimerMax);
                    if (burningTime > burningRecipeSO.burningTimerMax)
                    {
                        GetKitchenObj().DestroySelf();
                        KitchenObj.SpawnKitchenObj(burningRecipeSO.output, this);
                        state = State.Burned;
                        OnStateChanged?.Invoke(state);
                        OnProgressChanged?.Invoke(0f);
                        
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
                    OnStateChanged?.Invoke(state);
                    fryingTimer = 0;
                }
            }
            return;
        }

        // 情况 2：柜台有东西 → 仅当玩家空手时才递给他
        if (!player.HasKitchenObj())
        {
            GetKitchenObj().SetKitchenObjParent(player);
            state = State.Idle;
            OnStateChanged?.Invoke(state);
            OnProgressChanged?.Invoke(0f);
         
        }
    }

    // 判断是否有这个SO配方
    private bool HasFryRecipeWithInput(KitchenObjSO input)
    {
        FryRecipeSO fryRecipeSO = GetFryRecipeSOWithInput(input);
        return fryRecipeSO != null;
        
    }
    // 根据输入食材获取输出食材
    private KitchenObjSO GetOutputForInput(KitchenObjSO input)
    {
        FryRecipeSO fryRecipeSO = GetFryRecipeSOWithInput(input);
        if(fryRecipeSO != null)
        {
            return fryRecipeSO.output;
        }
        return null;
    }
    //拿到配方SO
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
    //拿到烧焦配方SO
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjSO input)
    {
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if(burningRecipeSO.input == input)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
}
