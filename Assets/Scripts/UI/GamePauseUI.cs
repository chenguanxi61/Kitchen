using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    public Button MainMenuButton;
    public Button ResumeButton;

    private void Awake()
    {
        MainMenuButton.onClick.AddListener(() =>
        {
            MainMenuButton.interactable = false;
            ResumeButton.interactable = false;
            StartCoroutine(LeaveNetworkSessionAndLoadMainMenu());
        });

        ResumeButton.onClick.AddListener(() =>
            GameManager.Instance.PauseGame());
    }

    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;
        Hide();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnGamePaused -= GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused -= GameManager_OnGameUnPaused;
    }

    private IEnumerator LeaveNetworkSessionAndLoadMainMenu()
    {
        Time.timeScale = 1f;
        KitchGameMultiPlayer.MarkLocalClientLeftSession();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();

            while (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                yield return null;
            }
        }

        Loader.Load(Loader.Scene.MainMenu);
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
