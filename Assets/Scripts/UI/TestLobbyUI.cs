using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLobbyUI : MonoBehaviour
{
    [SerializeField] Button createGameButton;
    [SerializeField] Button joinGameButton;


    private void Awake()
    {
        createGameButton.onClick.AddListener(() => {
            KitchGameMultiPlayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });
        
        joinGameButton.onClick.AddListener((() =>
                KitchGameMultiPlayer.Instance.StartClient()));
    }
}
