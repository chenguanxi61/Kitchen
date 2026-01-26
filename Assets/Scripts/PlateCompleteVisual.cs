using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlateCompleteVisual : MonoBehaviour
{ 
    [Serializable]
   public struct KitchenObjectSO_Hamburger//对应汉堡里面 obj和SO
    {
        public KitchenObjSO kitchenObjSO;
        public GameObject gameObject;
    }
    
    [SerializeField] private PlateKitchObj plateKitchObj;
    [SerializeField] private List<KitchenObjectSO_Hamburger> kitchenObjectSO_HamburgerList;

    public void Awake()
    {
        
    }

    private void Start()
    {
        plateKitchObj.OnAddSomething += PlateKitchObj_OnAddSomething;
        foreach (KitchenObjectSO_Hamburger so in kitchenObjectSO_HamburgerList)
        {
            so.gameObject.SetActive(false);
        }
    }
    
    private void PlateKitchObj_OnAddSomething(KitchenObjSO kitchenObjSO)
    {
        foreach (KitchenObjectSO_Hamburger so in kitchenObjectSO_HamburgerList)
        {
            if (so.kitchenObjSO == kitchenObjSO)
            {
                so.gameObject.SetActive(true);
            }
        }
    }

   
}
