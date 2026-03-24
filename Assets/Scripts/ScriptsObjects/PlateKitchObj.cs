using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchObj : KitchenObj
{
    [SerializeField] private List<KitchenObjSO> validKitchenObjSOList;

    public Action<KitchenObjSO> OnAddSomething;

    private NetworkList<int> plateKitchenObjIndexList;

    protected override void Awake()
    {
        base.Awake();
        plateKitchenObjIndexList = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        plateKitchenObjIndexList.OnListChanged += PlateKitchenObjIndexList_OnListChanged;
    }

    public override void OnNetworkDespawn()
    {
        plateKitchenObjIndexList.OnListChanged -= PlateKitchenObjIndexList_OnListChanged;
    }

    public bool TryAddSomething(KitchenObjSO kitchenObjSO)
    {
        if (!validKitchenObjSOList.Contains(kitchenObjSO))
        {
            return false;
        }

        if (ContainsKitchenObj(kitchenObjSO))
        {
            return false;
        }

        int kitchenObjIndex = KitchGameMultiPlayer.Instance.GetKitchenObjIndex(kitchenObjSO);
        AddKitchenObjServerRpc(kitchenObjIndex);
        return true;
    }

    public List<KitchenObjSO> GetKitchenObjSOList()
    {
        List<KitchenObjSO> kitchenObjSOList = new List<KitchenObjSO>();

        foreach (int kitchenObjIndex in plateKitchenObjIndexList)
        {
            kitchenObjSOList.Add(KitchGameMultiPlayer.Instance.GetKitchenObjByIndex(kitchenObjIndex));
        }

        return kitchenObjSOList;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddKitchenObjServerRpc(int kitchenObjIndex)
    {
        KitchenObjSO kitchenObjSO = KitchGameMultiPlayer.Instance.GetKitchenObjByIndex(kitchenObjIndex);

        if (!validKitchenObjSOList.Contains(kitchenObjSO))
        {
            return;
        }

        if (ContainsKitchenObj(kitchenObjSO))
        {
            return;
        }

        plateKitchenObjIndexList.Add(kitchenObjIndex);
    }

    private bool ContainsKitchenObj(KitchenObjSO kitchenObjSO)
    {
        foreach (int kitchenObjIndex in plateKitchenObjIndexList)
        {
            if (KitchGameMultiPlayer.Instance.GetKitchenObjByIndex(kitchenObjIndex) == kitchenObjSO)
            {
                return true;
            }
        }

        return false;
    }

    private void PlateKitchenObjIndexList_OnListChanged(NetworkListEvent<int> changeEvent)
    {
        if (changeEvent.Type != NetworkListEvent<int>.EventType.Add)
        {
            return;
        }

        KitchenObjSO kitchenObjSO = KitchGameMultiPlayer.Instance.GetKitchenObjByIndex(changeEvent.Value);
        OnAddSomething?.Invoke(kitchenObjSO);
    }
}
