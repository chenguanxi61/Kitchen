using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSelectCharacterUI : MonoBehaviour
{
   [SerializeField]private Button readyBtn;

   public void Awake()
   {
      readyBtn.onClick.AddListener((() =>
          KitchGameMultiPlayer.Instance.SetPlayerReady()));
   }
}
