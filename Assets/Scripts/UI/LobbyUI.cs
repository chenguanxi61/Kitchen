using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        quickJoinButton.interactable = !KitchGameMultiPlayer.HasLocalClientLeftSession;

        mainMenuButton.onClick.AddListener(() =>
            Loader.Load(Loader.Scene.MainMenu));

        quickJoinButton.onClick.AddListener(() =>
        {
            if (!KitchGameMultiPlayer.Instance.StartClient())
            {
                quickJoinButton.interactable = false;
            }
        });

        createButton.onClick.AddListener(() =>
        {
            KitchGameMultiPlayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        });
    }
}
