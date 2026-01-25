using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BurningRecipeSO : ScriptableObject
{
    public KitchenObjSO input;
    public KitchenObjSO output;
    public float burningTimerMax;
    
}
