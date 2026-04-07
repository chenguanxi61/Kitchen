using System;
using UnityEngine;
using UnityEngine.Events;

public class CuttingCounter : BaseCounter, IHasProgressBar
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipesSOArray;

    private int cuttingProgress;

    public event UnityAction<float> OnProgressChanged;
    public static Action<Vector3> OnCutting;

    public override void Interact(Player player)
    {
        if (!HasKitchenObj())
        {
            if (player.HasKitchenObj() && HasRecipeWithInput(player.GetKitchenObj().GetKitchenObjSO()))
            {
                player.GetKitchenObj().SetKitchenObjParent(this);
                ResetProgress();
            }

            return;
        }

        if (!player.HasKitchenObj())
        {
            GetKitchenObj().SetKitchenObjParent(player);
            ResetProgress();
            return;
        }

        if (player.GetKitchenObj().TryGetPlate(out PlateKitchObj plateKitchObj))
        {
            if (plateKitchObj.TryAddSomething(GetKitchenObj().GetKitchenObjSO()))
            {
                KitchenObj.DestoryKitchenObj(GetKitchenObj());
                ResetProgress();
            }
        }
    }

    public override void InteractAlternate(Player player)
    {
        if (!HasKitchenObj() || !HasRecipeWithInput(GetKitchenObj().GetKitchenObjSO()))
        {
            return;
        }

        cuttingProgress++;
        CuttingRecipeSO recipe = GetCuttingRecipeSO(GetKitchenObj().GetKitchenObjSO());
        OnProgressChanged?.Invoke((float)cuttingProgress / recipe.cuttingProgressMax);
        OnCutting?.Invoke(transform.position);

        if (cuttingProgress >= recipe.cuttingProgressMax)
        {
            KitchenObjSO outputKitchenObj = GetCuttingObjSO(GetKitchenObj().GetKitchenObjSO());
            KitchenObj.DestoryKitchenObj(GetKitchenObj());
            ResetProgress();
            KitchenObj.SpawnKitchenObj(outputKitchenObj, this);
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

    private void ResetProgress()
    {
        cuttingProgress = 0;
        OnProgressChanged?.Invoke(0f);
    }
}
