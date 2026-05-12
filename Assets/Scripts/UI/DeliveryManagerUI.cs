using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform itemTemplate;

    private void Awake()
    {
        itemTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        DeliverManager.Instance.OnRecipeSpawned += DeliverManager_OnRecipeChanged;
        DeliverManager.Instance.OnRecipeCompleted += DeliverManager_OnRecipeChanged;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (DeliverManager.Instance == null)
        {
            return;
        }

        DeliverManager.Instance.OnRecipeSpawned -= DeliverManager_OnRecipeChanged;
        DeliverManager.Instance.OnRecipeCompleted -= DeliverManager_OnRecipeChanged;
    }

    private void UpdateVisual()
    {
        foreach (Transform child in container)
        {
            if (child == itemTemplate)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        foreach (DeliveryOrder order in DeliverManager.Instance.GetWaitingOrderList())
        {
            Transform itemTransform = Instantiate(itemTemplate, container);
            itemTransform.gameObject.SetActive(true);
            itemTransform.GetComponent<DeliveryManagerSingleUI>().SetOrder(order);
        }
    }

    private void DeliverManager_OnRecipeChanged()
    {
        UpdateVisual();
    }
}
