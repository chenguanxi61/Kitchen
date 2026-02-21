using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Text recopesDeliveredText;

    public void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }
    
    private void GameManager_OnStateChanged(GameManager.State state)
    {
        if (GameManager.Instance.IsOver())
        {
            recopesDeliveredText.text = DeliverManager.Instance.GetSuccessfulDeliveries().ToString();
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Update()
    {
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }


}
