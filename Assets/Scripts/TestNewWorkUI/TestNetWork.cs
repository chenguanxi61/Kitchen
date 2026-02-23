using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
    
public class TestNetWork : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startclientButton;
    private void Awake()
    {
        startHostButton.onClick.AddListener(() =>
        {
            Debug.Log("Start Host");
            NetworkManager.Singleton.StartHost();
            Hide();
        });
        startclientButton.onClick.AddListener(() =>
        {
            Debug.Log("Start Client");
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
