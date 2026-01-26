using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private PlateKitchObj plateKitchObj;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }
    
    void Start()
    {
        plateKitchObj.OnAddSomething += PlateKitchObj_OnAddSomething;
    }

    private void PlateKitchObj_OnAddSomething(KitchenObjSO kitchenObjSO)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in transform)
        {
            if(child==iconTemplate) continue;
            Destroy(child.gameObject);
        }
        foreach (KitchenObjSO kitchenObjSO  in plateKitchObj.GetKitchenObjSOList())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<PlateIconSingleUI>().SetKitchenObjSO(kitchenObjSO);
            
        }
    }
}
