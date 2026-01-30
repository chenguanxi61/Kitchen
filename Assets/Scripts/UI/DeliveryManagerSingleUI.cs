using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    
    [SerializeField] private Text nameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    public void SetTemplate(RecipeSO recipeSO)
    {
        nameText.text = recipeSO.name;
        foreach (Transform child in iconContainer)
        {
            if(child==iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjSO kitchenObjSo in recipeSO.kitchenObjSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite= kitchenObjSo.sprite;
            
        }
        
    }
    
    
}
