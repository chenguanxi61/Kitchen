using Unity.Netcode;
using UnityEngine;

public class KitchenObj : NetworkBehaviour
{
    [SerializeField] private KitchenObjSO kitchenObjSO;

    private IKitchObjParent iKitchObjParent;
    private FollowTransfrom followTransfrom;

    protected virtual void Awake()
    {
        followTransfrom = GetComponent<FollowTransfrom>();
    }

    public KitchenObjSO GetKitchenObjSO()
    {
        return kitchenObjSO;
    }

    public void SetKitchenObjParent(IKitchObjParent newKitchenObjParent)
    {
        SetKitchenObjParentServerRpc(newKitchenObjParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjParentServerRpc(NetworkObjectReference kitchenObjParentNetworkReference)
    {
        SetKitchenObjParentClientRpc(kitchenObjParentNetworkReference);
    }

    [ClientRpc]
    private void SetKitchenObjParentClientRpc(NetworkObjectReference kitchenObjParentNetworkReference)
    {
        kitchenObjParentNetworkReference.TryGet(out NetworkObject kitchenObjParentNetworkObj);
        IKitchObjParent newKitchenObjParent = kitchenObjParentNetworkObj.GetComponent<IKitchObjParent>();

        if (iKitchObjParent != null)
        {
            iKitchObjParent.ClearKitchenObj();
        }

        iKitchObjParent = newKitchenObjParent;

        if (newKitchenObjParent.HasKitchenObj())
        {
            Debug.LogError("The new kitchen object parent already has a kitchen object.");
        }

        newKitchenObjParent.SetKitchenObj(this);
        followTransfrom.SetTargetTransfrom(newKitchenObjParent.GetTopPoint());
    }

    public IKitchObjParent GetKitchenObjParent()
    {
        return iKitchObjParent;
    }

    public void DestroySelf()
    {
        if (iKitchObjParent != null)
        {
            ClearKitchenObjOnParent();
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            if (IsServer)
            {
                NetworkObject.Despawn(true);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClearKitchenObjOnParent()
    {
        if (iKitchObjParent == null)
        {
            return;
        }

        iKitchObjParent.ClearKitchenObj();
        iKitchObjParent = null;
    }

    public static void SpawnKitchenObj(KitchenObjSO kitchenObjSO, IKitchObjParent kitchenObjParent)
    {
        KitchGameMultiPlayer.Instance.SpawnKitchenObj(kitchenObjSO, kitchenObjParent);
    }

    public bool TryGetPlate(out PlateKitchObj plateKitchObj)
    {
        if (this is PlateKitchObj)
        {
            plateKitchObj = this as PlateKitchObj;
            return true;
        }

        plateKitchObj = null;
        return false;
    }

    public static void DestoryKitchenObj(KitchenObj kitchenObj)
    {
        KitchGameMultiPlayer.Instance.DestroyKitchenObj(kitchenObj);
    }
}
