using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlatesCounter : BaseCounter
{
    public UnityAction OnPlateSpawned;
    public UnityAction OnPlateRemoved;

    [SerializeField] private float spawnPlateTimer;
    [SerializeField] private float spawnPlateTimerMax = 4f;
    [SerializeField] private KitchenObjSO plateKitchenObjSO;

    private readonly NetworkVariable<int> plateSpawnedAmountNetworkVariable = new NetworkVariable<int>(0);
    private int plateSpawnedAmountMax = 4;

    public override void OnNetworkSpawn()
    {
        plateSpawnedAmountNetworkVariable.OnValueChanged += PlateSpawnedAmountNetworkVariable_OnValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        plateSpawnedAmountNetworkVariable.OnValueChanged -= PlateSpawnedAmountNetworkVariable_OnValueChanged;
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer >= spawnPlateTimerMax)
        {
            spawnPlateTimer = 0f;
            if (plateSpawnedAmountNetworkVariable.Value < plateSpawnedAmountMax)
            {
                plateSpawnedAmountNetworkVariable.Value++;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (IsServer)
        {
            InteractInternal(player);
        }
        else
        {
            InteractServerRpc(player.NetworkObject);
        }
    }

    public int GetPlateSpawnedAmount()
    {
        return plateSpawnedAmountNetworkVariable.Value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        if (!playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject))
        {
            return;
        }

        Player player = playerNetworkObject.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        InteractInternal(player);
    }

    private void InteractInternal(Player player)
    {
        if (!player.HasKitchenObj() && plateSpawnedAmountNetworkVariable.Value > 0)
        {
            KitchenObj.SpawnKitchenObj(plateKitchenObjSO, player);
            plateSpawnedAmountNetworkVariable.Value--;
        }
    }

    private void PlateSpawnedAmountNetworkVariable_OnValueChanged(int previousValue, int newValue)
    {
        int amountDelta = newValue - previousValue;

        for (int i = 0; i < amountDelta; i++)
        {
            OnPlateSpawned?.Invoke();
        }

        for (int i = 0; i < -amountDelta; i++)
        {
            OnPlateRemoved?.Invoke();
        }
    }
}
