using UnityEngine;

public class DeliveryOrder
{
    public RecipeSO Recipe { get; }
    public float TimeRemaining { get; private set; }
    public float TimeMax { get; }

    public DeliveryOrder(RecipeSO recipe)
    {
        Recipe = recipe;
        TimeMax = Mathf.Max(1f, recipe.orderTimeMax);
        TimeRemaining = TimeMax;
    }

    public void Tick(float deltaTime)
    {
        TimeRemaining = Mathf.Max(0f, TimeRemaining - deltaTime);
    }

    public float GetTimeNormalized()
    {
        return TimeRemaining / TimeMax;
    }

    public int GetCurrentScore()
    {
        return Mathf.RoundToInt(Mathf.Lerp(Recipe.scoreMin, Recipe.scoreMax, GetTimeNormalized()));
    }
}
