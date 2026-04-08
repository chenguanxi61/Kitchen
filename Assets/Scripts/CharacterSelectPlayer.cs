using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{

    [SerializeField] private int PlayerIndex;
    [SerializeField] private PlayerVisual playerVisual;
    private Color32 lastAppliedColor;
    private bool hasLastAppliedColor;

    private void Awake()
    {
        if (playerVisual == null)
        {
            playerVisual = GetComponentInChildren<PlayerVisual>();
        }
    }

    public int GetPlayerIndex()
    {
        return PlayerIndex;
    }

    public void Start()
    {
        KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged += InstanceOnOnPlayerDataNetWorkListChanged;
        UpdatePlayer();
    }

    private void OnDestroy()
    {
        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged -= InstanceOnOnPlayerDataNetWorkListChanged;
        }
    }

    private void InstanceOnOnPlayerDataNetWorkListChanged()
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        bool isConnected = KitchGameMultiPlayer.Instance.IsPlayerIndexConnected(PlayerIndex);
        if (isConnected)
        {
            Show();
        }
        else
        {
            Hide();
        }

        if (isConnected && playerVisual != null)
        {
            if (!KitchGameMultiPlayer.Instance.TryGetPlayerClientIdByIndex(PlayerIndex, out ulong clientId))
            {
                return;
            }

            Color32 targetColor = KitchGameMultiPlayer.Instance.GetPlayerColor(clientId);
            if (!hasLastAppliedColor ||
                lastAppliedColor.r != targetColor.r ||
                lastAppliedColor.g != targetColor.g ||
                lastAppliedColor.b != targetColor.b)
            {
                playerVisual.SetPlayerColor(targetColor);
                lastAppliedColor = targetColor;
                hasLastAppliedColor = true;
            }
        }
    }

    private void LateUpdate()
    {
        if (KitchGameMultiPlayer.Instance == null)
        {
            return;
        }

        UpdatePlayer();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
