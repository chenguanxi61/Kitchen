using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class DeliveryManagerUI : MonoBehaviour
{
    
    
    [SerializeField] private Transform container;
    [SerializeField] private Transform itemTemplate;//菜品模板


    private void Awake()
    {
        itemTemplate.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        DeliverManager.Instance.OnRecipeSpawned += DeliverManager_OnRecipeSpawned;
        DeliverManager.Instance.OnRecipeCompleted += DeliverManager_OnRecipeCompleted;
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        foreach (Transform child in container)
        {
            if(child==itemTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (RecipeSO recipeSo in DeliverManager.Instance.GetWaitingRecipeSOList())
        {
            //在container下实例化菜品模板
            Transform itemTransform = Instantiate(itemTemplate, container);
            itemTransform.gameObject.SetActive(true);
            itemTransform.GetComponent<DeliveryManagerSingleUI>().SetTemplate(recipeSo);
            
        }
       
    }
    
    private void DeliverManager_OnRecipeSpawned()
    {
        UpdateVisual();
    }
    
    private void DeliverManager_OnRecipeCompleted()
    {
        UpdateVisual();
    }   
}
