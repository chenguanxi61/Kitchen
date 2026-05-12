using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
    public List<KitchenObjSO> kitchenObjSOList;
    public string name;
    public float orderTimeMax = 35f;
    public int scoreMax = 150;
    public int scoreMin = 50;
}
