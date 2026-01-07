using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour ,IKitchObjParent
{
    
    public static Player Instance{get; private set;}
    
    
    
    [SerializeField] private GameInput gameInput;
    [SerializeField] private float speed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    
    [SerializeField] private KitchenObj kitchenObj;
    [SerializeField] private Transform kitchenObjHoldPoint;
    
    public event EventHandler<OnSelectedCounterChangedEvent> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEvent : EventArgs
    {
        public BaseCounter SelectedCounter;
    }
    
    
    private Vector3 lastInteractDir;

    private bool isWalking = false;

    private BaseCounter selectedCounter;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("已经存在Player实例");
        }
        Instance = this;
    }
    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if(selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }
    

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }
    //交互处理
    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMoveVectorNormalized();
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
                // 命中柜台 → 始终选中它（如果不是当前选中的）
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                // 命中非柜台 → 清空选中
                SetSelectedCounter(null);
            }
        }
        else
        {
            // 射线没有打到任何东西 → 清空选中
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMoveVectorNormalized();
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

        // 斜向移动被阻挡 → 尝试 X / Z 单轴移动
        if (!canMove && moveDir != Vector3.zero)
        {
            // X 轴
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
                // Z 轴
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

        // walking 状态
        isWalking = canMove && inputVector != Vector2.zero;
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
}
