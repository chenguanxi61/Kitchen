using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Image timeSliderImage;

    private DeliveryOrder order;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);

        if (timeSliderImage == null)
        {
            Transform timeSliderTransform = transform.Find("TimeSlider");
            if (timeSliderTransform != null)
            {
                timeSliderImage = timeSliderTransform.GetComponent<Image>();
            }
        }
    }

    private void Update()
    {
        if (order == null || timeSliderImage == null)
        {
            return;
        }

        float timeNormalized = order.GetTimeNormalized();
        timeSliderImage.fillAmount = timeNormalized;
        timeSliderImage.color = Color.Lerp(Color.red, Color.green, timeNormalized);
    }

    public void SetOrder(DeliveryOrder order)
    {
        this.order = order;
        SetTemplate(order.Recipe);
        Update();
    }

    public void SetTemplate(RecipeSO recipeSO)
    {
        nameText.text = recipeSO.name;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        foreach (KitchenObjSO kitchenObjSO in recipeSO.kitchenObjSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjSO.sprite;
        }
    }
}
