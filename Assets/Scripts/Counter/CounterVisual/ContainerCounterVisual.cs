using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


struct containerAni
{
    public const string OpenClose = "OpenClose";
}
public class ContainerCounterVisual : MonoBehaviour
{
   [SerializeField] private ContainerCounter containerCounter;
   private Animator animator;

   private void Awake()
   {
      animator = GetComponent<Animator>();
      
   }

   private void Start()
   {
      containerCounter.OnPLayerGetKitchenObj += ContainerCounter_OnPLayerGetKitchenObj;
   }

   private void ContainerCounter_OnPLayerGetKitchenObj(object sender, EventArgs e)
   {
      animator.SetTrigger(containerAni.OpenClose);
   }
}
