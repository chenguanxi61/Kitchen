using UnityEngine;

public class CharacterSelectPlayerReadyVisual : MonoBehaviour
{
    [SerializeField] private GameObject readyGameObject;

    private CharacterSelectPlayer characterSelectPlayer;

    private void Awake()
    {
        characterSelectPlayer = GetComponent<CharacterSelectPlayer>();
    }

    private void Start()
    {
        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged += Instance_OnPlayerDataNetWorkListChanged;
        }

        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged -= Instance_OnPlayerDataNetWorkListChanged;
        }
    }

    private void Instance_OnPlayerDataNetWorkListChanged()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (readyGameObject == null || characterSelectPlayer == null || KitchGameMultiPlayer.Instance == null)
        {
            return;
        }

        int playerIndex = characterSelectPlayer.GetPlayerIndex();
        bool isConnected = KitchGameMultiPlayer.Instance.IsPlayerIndexConnected(playerIndex);
        bool isReady = KitchGameMultiPlayer.Instance.IsPlayerIndexReady(playerIndex);
        readyGameObject.SetActive(isConnected && isReady);
    }
}
