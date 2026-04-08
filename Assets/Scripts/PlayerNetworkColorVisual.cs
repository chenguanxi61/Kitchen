using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkColorVisual : NetworkBehaviour
{
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged += Instance_OnPlayerDataNetWorkListChanged;
        }

        UpdateColor();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (KitchGameMultiPlayer.Instance != null)
        {
            KitchGameMultiPlayer.Instance.OnPlayerDataNetWorkListChanged -= Instance_OnPlayerDataNetWorkListChanged;
        }
    }

    private void Instance_OnPlayerDataNetWorkListChanged()
    {
        UpdateColor();
    }

    private void LateUpdate()
    {
        if (!IsSpawned)
        {
            return;
        }

        UpdateColor();
    }

    private void UpdateColor()
    {
        if (playerVisual == null || KitchGameMultiPlayer.Instance == null)
        {
            return;
        }

        Color32 targetColor = KitchGameMultiPlayer.Instance.GetPlayerColor(OwnerClientId);
        if (hasLastAppliedColor &&
            lastAppliedColor.r == targetColor.r &&
            lastAppliedColor.g == targetColor.g &&
            lastAppliedColor.b == targetColor.b)
        {
            return;
        }

        playerVisual.SetPlayerColor(targetColor);
        lastAppliedColor = targetColor;
        hasLastAppliedColor = true;
    }
}
