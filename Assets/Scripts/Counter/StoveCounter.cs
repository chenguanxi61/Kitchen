using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class StoveCounter : BaseCounter, IHasProgressBar
{
    public event UnityAction<float> OnProgressChanged;
    public UnityAction<State> OnStateChanged;

    public static Action<Vector3> OnAnyObjectFried;

    public enum State
    {
        Idle,
        Frying,
        Fryed,
        Burned,
    }

    [SerializeField] private FryRecipeSO[] fryRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private readonly NetworkVariable<int> stateNetworkVariable = new NetworkVariable<int>((int)State.Idle);
    private readonly NetworkVariable<float> progressNetworkVariable = new NetworkVariable<float>(0f);

    private float fryingTimer;
    private FryRecipeSO fryRecipeSO;
    private float burningTime;
    private BurningRecipeSO burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        stateNetworkVariable.OnValueChanged += StateNetworkVariable_OnValueChanged;
        progressNetworkVariable.OnValueChanged += ProgressNetworkVariable_OnValueChanged;

        OnStateChanged?.Invoke(GetState());
        OnProgressChanged?.Invoke(progressNetworkVariable.Value);
    }

    public override void OnNetworkDespawn()
    {
        stateNetworkVariable.OnValueChanged -= StateNetworkVariable_OnValueChanged;
        progressNetworkVariable.OnValueChanged -= ProgressNetworkVariable_OnValueChanged;
    }

    private void Update()
    {
        if (!IsServer || !HasKitchenObj())
        {
            return;
        }

        switch (GetState())
        {
            case State.Idle:
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;
                SetProgress(fryingTimer / fryRecipeSO.fryTimerMax);

                if (fryingTimer > fryRecipeSO.fryTimerMax)
                {
                    KitchenObjSO cookedKitchenObjSO = fryRecipeSO.output;
                    KitchenObj.DestoryKitchenObj(GetKitchenObj());
                    KitchenObj.SpawnKitchenObj(cookedKitchenObjSO, this);

                    burningTime = 0f;
                    burningRecipeSO = GetBurningRecipeSOWithInput(cookedKitchenObjSO);
                    SetState(State.Fryed);
                    SetProgress(burningRecipeSO == null ? 1f : 0f);
                }
                break;
            case State.Fryed:
                if (burningRecipeSO == null)
                {
                    return;
                }

                burningTime += Time.deltaTime;
                SetProgress(burningTime / burningRecipeSO.burningTimerMax);

                if (burningTime > burningRecipeSO.burningTimerMax)
                {
                    KitchenObj.DestoryKitchenObj(GetKitchenObj());
                    KitchenObj.SpawnKitchenObj(burningRecipeSO.output, this);
                    SetState(State.Burned);
                    SetProgress(0f);
                }
                break;
            case State.Burned:
                break;
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
        if (!HasKitchenObj())
        {
            if (!player.HasKitchenObj())
            {
                return;
            }

            KitchenObjSO input = player.GetKitchenObj().GetKitchenObjSO();
            FryRecipeSO matchedRecipeSO = GetFryRecipeSOWithInput(input);

            if (matchedRecipeSO == null)
            {
                return;
            }

            player.GetKitchenObj().SetKitchenObjParent(this);
            fryRecipeSO = matchedRecipeSO;
            fryingTimer = 0f;
            burningTime = 0f;
            burningRecipeSO = null;
            SetState(State.Frying);
            SetProgress(0f);
            return;
        }

        if (!player.HasKitchenObj())
        {
            GetKitchenObj().SetKitchenObjParent(player);
            ResetStove();
            return;
        }

        if (player.GetKitchenObj().TryGetPlate(out PlateKitchObj plateKitchObj))
        {
            if (plateKitchObj.TryAddSomething(GetKitchenObj().GetKitchenObjSO()))
            {
                KitchenObj.DestoryKitchenObj(GetKitchenObj());
                ResetStove();
            }
        }
    }

    private void ResetStove()
    {
        fryingTimer = 0f;
        burningTime = 0f;
        fryRecipeSO = null;
        burningRecipeSO = null;
        SetState(State.Idle);
        SetProgress(0f);
    }

    private void SetState(State state)
    {
        stateNetworkVariable.Value = (int)state;
    }

    private State GetState()
    {
        return (State)stateNetworkVariable.Value;
    }

    public State GetCurrentState()
    {
        return GetState();
    }

    public float GetCurrentProgress()
    {
        return progressNetworkVariable.Value;
    }

    private void SetProgress(float progress)
    {
        progressNetworkVariable.Value = Mathf.Clamp01(progress);
    }

    private void StateNetworkVariable_OnValueChanged(int previousValue, int newValue)
    {
        OnStateChanged?.Invoke((State)newValue);
    }

    private void ProgressNetworkVariable_OnValueChanged(float previousValue, float newValue)
    {
        OnProgressChanged?.Invoke(newValue);
    }

    private FryRecipeSO GetFryRecipeSOWithInput(KitchenObjSO input)
    {
        foreach (FryRecipeSO currentFryRecipeSO in fryRecipeSOArray)
        {
            if (currentFryRecipeSO.input == input)
            {
                return currentFryRecipeSO;
            }
        }

        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjSO input)
    {
        foreach (BurningRecipeSO currentBurningRecipeSO in burningRecipeSOArray)
        {
            if (currentBurningRecipeSO.input == input)
            {
                return currentBurningRecipeSO;
            }
        }

        return null;
    }
}
