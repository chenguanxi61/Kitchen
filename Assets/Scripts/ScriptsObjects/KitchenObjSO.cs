using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "KitchenObjSO", menuName = "KitchenObjSO")]
public class KitchenObjSO : ScriptableObject
{
   public Transform prefab;
   public Sprite sprite;
   public string objName;
}
