using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text;
    
    public void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        Hide();
    }
    
    private void GameManager_OnStateChanged(GameManager.State state)
    {
        if (state == GameManager.State.CountdownToStart)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Update()
    {
        Text.text = Math.Ceiling(GameManager.Instance.GetCountDownToStartTimer()).ToString();
        Debug.Log(Text.text);
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
