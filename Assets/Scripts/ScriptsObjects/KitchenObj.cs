using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObj : MonoBehaviour
{
    [SerializeField] private KitchenObjSO kitchenObjSO;
    //判断谁拿到了自己
    private IKitchObjParent iKitchObjParent;
    
    public KitchenObjSO GetKitchenObjSO()
    {
        return kitchenObjSO;
    }
    
    public void SetKitchenObjParent(IKitchObjParent newKitchenObjParent)
    {
        // 1. 清理旧桌台
        if (iKitchObjParent != null)
        {
            iKitchObjParent.ClearKitchenObj();
        }

        // 2. 切换到新桌台
        iKitchObjParent = newKitchenObjParent;

        // 3. 新桌台不应当有 KitchenObj
        if (newKitchenObjParent.HasKitchenObj())
        {
            Debug.LogError("新的父物体已经有物品了！");
        }

        // 4. 给新桌台赋值
        newKitchenObjParent.SetKitchenObj(this);

        // 5. 设置物体位置
        transform.parent = newKitchenObjParent.GetTopPoint();
        transform.localPosition = Vector3.zero;
    }
    
    

    public IKitchObjParent GetKitchenObjParent()
    {
        return iKitchObjParent;
    }
    
    
    public void DestroySelf()
    {
        // 1. 清理旧桌台
        if (iKitchObjParent != null)
        {
            iKitchObjParent.ClearKitchenObj();
        }
        // 2. 销毁物体
        Destroy(gameObject);
    }
    
    public static KitchenObj SpawnKitchenObj(KitchenObjSO kitchenObjSO,IKitchObjParent kitchenObjParent)
    {
        Transform obj = Instantiate(kitchenObjSO.prefab);
        obj.transform.GetComponent<KitchenObj>().SetKitchenObjParent(kitchenObjParent);
        return obj.transform.GetComponent<KitchenObj>();
    }
    
    public bool TryGetPlate(out PlateKitchObj plateKitchObj)
    {
        if (this is PlateKitchObj)
        {
            plateKitchObj = this as PlateKitchObj;
            return true;
        }
        else
        {
            plateKitchObj = null;
            return false;
        }
    }
}
