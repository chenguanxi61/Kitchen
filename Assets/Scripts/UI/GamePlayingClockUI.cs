using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
     public Image timeFillImage;

    private void Update()
    {
        timeFillImage.fillAmount = GameManager.Instance.GetPlayingTimerNormalized();
    }
}
