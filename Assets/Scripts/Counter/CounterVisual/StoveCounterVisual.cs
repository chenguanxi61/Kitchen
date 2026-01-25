using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
   [SerializeField] private StoveCounter stoveCounter;
   [SerializeField] private GameObject stoveOnGameObject;
   [SerializeField] private GameObject particlesGameObject;

   private void Start()
   {
      stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
   }
   
   private void StoveCounter_OnStateChanged(StoveCounter.State state)
   {
      bool showVisual = state == StoveCounter.State.Frying || state == StoveCounter.State.Fryed;
      stoveOnGameObject.SetActive(showVisual);
      particlesGameObject.SetActive(showVisual);
   }
}
