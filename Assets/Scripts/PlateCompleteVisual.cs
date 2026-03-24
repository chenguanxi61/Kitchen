using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSO_Hamburger
    {
        public KitchenObjSO kitchenObjSO;
        public GameObject gameObject;
    }

    [SerializeField] private PlateKitchObj plateKitchObj;
    [SerializeField] private List<KitchenObjectSO_Hamburger> kitchenObjectSO_HamburgerList;

    private void Start()
    {
        if (plateKitchObj == null)
        {
            return;
        }

        plateKitchObj.OnAddSomething += PlateKitchObj_OnAddSomething;
        ResetVisual();
        RefreshVisual();
    }

    private void OnDestroy()
    {
        if (plateKitchObj != null)
        {
            plateKitchObj.OnAddSomething -= PlateKitchObj_OnAddSomething;
        }
    }

    private void PlateKitchObj_OnAddSomething(KitchenObjSO kitchenObjSO)
    {
        RefreshVisual();
    }

    private void ResetVisual()
    {
        foreach (KitchenObjectSO_Hamburger visualEntry in kitchenObjectSO_HamburgerList)
        {
            visualEntry.gameObject.SetActive(false);
        }
    }

    private void RefreshVisual()
    {
        if (plateKitchObj == null)
        {
            return;
        }

        ResetVisual();

        foreach (KitchenObjSO kitchenObjSO in plateKitchObj.GetKitchenObjSOList())
        {
            foreach (KitchenObjectSO_Hamburger visualEntry in kitchenObjectSO_HamburgerList)
            {
                if (visualEntry.kitchenObjSO == kitchenObjSO)
                {
                    visualEntry.gameObject.SetActive(true);
                }
            }
        }
    }
}
