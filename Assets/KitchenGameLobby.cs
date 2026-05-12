using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    private Lobby joinedLobby;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private async Task<bool> InitializeUnityAuthentication()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                InitializationOptions options = new InitializationOptions();
                options.SetProfile(UnityEngine.Random.Range(0, 1000).ToString());
                await UnityServices.InitializeAsync(options);
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public async void CreatLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            if (!await InitializeUnityAuthentication())
            {
                return;
            }

            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                KitchGameMultiPlayer.MAX_PLAYERS,
                new CreateLobbyOptions { IsPrivate = isPrivate });

            KitchGameMultiPlayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            if (!await InitializeUnityAuthentication())
            {
                return;
            }

            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            KitchGameMultiPlayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
