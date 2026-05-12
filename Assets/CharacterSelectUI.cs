using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            mainMenuButton.interactable = false;
            readyButton.interactable = false;
            StartCoroutine(LeaveNetworkSessionAndLoadMainMenu());
        });

        readyButton.onClick.AddListener(() =>
            KitchGameMultiPlayer.Instance.SetPlayerReady());
    }

    private IEnumerator LeaveNetworkSessionAndLoadMainMenu()
    {
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
}
