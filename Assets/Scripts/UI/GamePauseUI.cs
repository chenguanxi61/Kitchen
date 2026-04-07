using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    public Button MainMenuButton;
    public Button ResumeButton;

    public void Awake()
    {
        MainMenuButton.onClick.AddListener((() => 
            Loader.Load(Loader.Scene.MainMenu)));
        ResumeButton.onClick.AddListener((() => 
            GameManager.Instance.PauseGame()));
    }

    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;
        Hide();
    }

    private void GameManager_OnGameUnPaused()
    {
        Hide();
    }
    private void GameManager_OnGamePaused()
    {
        Show();
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

