using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
    
public class TestNetWork : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. TestNetWork UI is disabled.");
            startHostButton.interactable = false;
            startClientButton.interactable = false;
            return;
        }

        startHostButton.onClick.AddListener(() => StartNetworkMode(NetworkManager.Singleton.StartHost, "Host"));
        startClientButton.onClick.AddListener(() => StartNetworkMode(NetworkManager.Singleton.StartClient, "Client"));
    }

    private void StartNetworkMode(System.Func<bool> startMode, string modeName)
    {
        bool startedSuccessfully = startMode();
        Debug.Log($"Start {modeName}: {(startedSuccessfully ? "Success" : "Failed")}");

        if (startedSuccessfully)
        {
            Hide();
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
