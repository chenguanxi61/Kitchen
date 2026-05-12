using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour ,IKitchObjParent
{
    
    public static Player LocalInstance{get; private set;}
    public static Action OnAnyPlayerSpawned;
    
    public static void ResetStaticData()
    {
        
        OnAnyPlayerSpawned = null;
    }
    
    
    [SerializeField] private float speed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    
    [SerializeField] private KitchenObj kitchenObj;
    [SerializeField] private Transform kitchenObjHoldPoint;
    
    public event EventHandler<OnSelectedCounterChangedEvent> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEvent : EventArgs
    {
        public BaseCounter SelectedCounter;
    }
    //音效----------------------------------------
    public Action<Vector3> OnPickedSomething;

    public Action<Vector3> OnMoving;
    //--------------------------------------------
    private Vector3 lastInteractDir;

    private bool isWalking = false;

    private BaseCounter selectedCounter;
    private bool isSubscribedToInput;

    public void Awake()
    {
        //Instance = this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
            OnAnyPlayerSpawned?.Invoke();
            TrySubscribeInput();
        }
    }

    public override void OnNetworkDespawn()
    {
        TryUnsubscribeInput();

        if (IsOwner && LocalInstance == this)
        {
            LocalInstance = null;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        TryUnsubscribeInput();
    }

    private void Start()
    {
        TrySubscribeInput();
    }

    private void TrySubscribeInput()
    {
        if (!IsOwner || isSubscribedToInput || GameInput.Instance == null)
        {
            return;
        }

        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
        isSubscribedToInput = true;
    }

    private void TryUnsubscribeInput()
    {
        if (!isSubscribedToInput || GameInput.Instance == null)
        {
            return;
        }

        GameInput.Instance.OnInteractAction -= GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction -= GameInput_OnInteractAlternateAction;
        isSubscribedToInput = false;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!CanControl())
        {
            return;
        }

        if(selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }
    

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (!CanControl())
        {
            return;
        }

        if(selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!CanControl())
        {
            isWalking = false;
            SetSelectedCounter(null);
            return;
        }

        //HandleMovementServerAuth();
        HandleMovement();
        HandleInteractions();
    }

    private bool CanControl()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.IsPlaying() &&
               !GameManager.Instance.IsPaused();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleMovementServerAuth()
    {
        Vector2 inputVector = GameInput.Instance.GetMoveVectorNormalized();
        HandleMovementServerRpc(inputVector);
    }
    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector2 inputVector)
    {
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        float moveDistance = speed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;

        bool canMove = moveDir != Vector3.zero &&
                       !Physics.CapsuleCast(transform.position, 
                                            transform.position + Vector3.up * playerHeight,
                                            playerRadius,
                                            moveDir,
                                            moveDistance);

        // 斜向移动被阻尝试 X / Z 单轴移动
        if (!canMove && moveDir != Vector3.zero)
        {
            // X 
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            if (moveDir.x != 0 &&
                !Physics.CapsuleCast(transform.position,
                                     transform.position + Vector3.up * playerHeight,
                                     playerRadius,
                                     moveDirX,
                                     moveDistance))
            {
                moveDir = moveDirX;
                canMove = true;
            }
            else
            {
                // Z �?
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                if (moveDir.z != 0 &&
                    !Physics.CapsuleCast(transform.position,
                                         transform.position + Vector3.up * playerHeight,
                                         playerRadius,
                                         moveDirZ,
                                         moveDistance))
                {
                    moveDir = moveDirZ;
                    canMove = true;
                }
            }
        }

        // 移动逻辑
        if (canMove)
        {
            transform.position += moveDir * moveDistance;

            if (moveDir != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(
                    transform.forward, 
                    moveDir,
                    Time.deltaTime * 10f
                );
            }
        }

        // walking 状�?
        isWalking = canMove && inputVector != Vector2.zero;
        if(isWalking)
        {
            OnMoving?.Invoke(transform.position);
        }
    }
    //交互处理
    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMoveVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;

        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            // 射线没有打到任何东西 清空选中
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMoveVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y).normalized;

        float moveDistance = speed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;

        bool canMove = moveDir != Vector3.zero &&
                       !Physics.CapsuleCast(transform.position, 
                                            transform.position + Vector3.up * playerHeight,
                                            playerRadius,
                                            moveDir,
                                            moveDistance);

        // 斜向移动被阻尝试 X / Z 单轴移动
        if (!canMove && moveDir != Vector3.zero)
        {
            // X 
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            if (moveDir.x != 0 &&
                !Physics.CapsuleCast(transform.position,
                                     transform.position + Vector3.up * playerHeight,
                                     playerRadius,
                                     moveDirX,
                                     moveDistance))
            {
                moveDir = moveDirX;
                canMove = true;
            }
            else
            {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                if (moveDir.z != 0 &&
                    !Physics.CapsuleCast(transform.position,
                                         transform.position + Vector3.up * playerHeight,
                                         playerRadius,
                                         moveDirZ,
                                         moveDistance))
                {
                    moveDir = moveDirZ;
                    canMove = true;
                }
            }
        }

        // 移动逻辑
        if (canMove)
        {
            transform.position += moveDir * moveDistance;

            if (moveDir != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(
                    transform.forward, 
                    moveDir,
                    Time.deltaTime * 10f
                );
            }
        }

        // walking 
        isWalking = canMove && inputVector != Vector2.zero;
        if(isWalking)
        {
            OnMoving?.Invoke(transform.position);
        }
    }
    
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEvent() { SelectedCounter = selectedCounter });
    }

    public Transform GetTopPoint()
    {
        return kitchenObjHoldPoint;
    }

    public void SetKitchenObj(KitchenObj kitchenObj)
    {
        this.kitchenObj = kitchenObj;
        if(kitchenObj!=null)
            OnPickedSomething?.Invoke(kitchenObj.transform.position);
    }

    public KitchenObj GetKitchenObj()
    {
        return kitchenObj;
    }

    public void ClearKitchenObj()
    {
        kitchenObj = null;
    }

    public bool HasKitchenObj()
    {
        return kitchenObj != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
