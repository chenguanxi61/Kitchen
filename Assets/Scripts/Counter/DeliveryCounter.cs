using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObj())
        {
            return;
        }

        if (player.GetKitchenObj().TryGetPlate(out PlateKitchObj plateKitchObj))
        {
            DeliverManager.Instance.DeliverRecipe(plateKitchObj);
        }
    }
}
