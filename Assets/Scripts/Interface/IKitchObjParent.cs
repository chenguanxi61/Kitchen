using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKitchObjParent
{
    public Transform GetTopPoint();
    public void SetKitchenObj(KitchenObj kitchenObj);
    public KitchenObj GetKitchenObj();
    public void ClearKitchenObj();

    public bool HasKitchenObj();


}
