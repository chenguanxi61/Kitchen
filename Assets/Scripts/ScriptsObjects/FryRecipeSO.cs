using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FryRecipeSO : ScriptableObject
{
    public KitchenObjSO input;
    public KitchenObjSO output;
    public float fryTimerMax;
    
}
