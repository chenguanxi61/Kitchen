using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
   private PlayerInputAction playerInputActions;
   
   public event EventHandler OnInteractAction;
   public event EventHandler OnInteractAlternateAction;
   private void Awake()
   {
      playerInputActions = new PlayerInputAction();
      playerInputActions.PLayer.Enable();
      playerInputActions.PLayer.Interact.performed += Interact_performed;
      
      playerInputActions.PLayer.InteractAlternate.performed += InteractAlternate_performed;
   }
   private void Interact_performed(InputAction.CallbackContext obj)
   {
      OnInteractAction?.Invoke(this,EventArgs.Empty);
   }
   private void InteractAlternate_performed(InputAction.CallbackContext obj)
   {
      OnInteractAlternateAction?.Invoke(this,EventArgs.Empty);
   }
   
   public Vector2 GetMoveVectorNormalized()
   {
      Vector2 inputVector = new Vector2 (0, 0);
      inputVector = playerInputActions.PLayer.Move.ReadValue<Vector2>();
      inputVector = inputVector.normalized;
      return inputVector;
   }
}
