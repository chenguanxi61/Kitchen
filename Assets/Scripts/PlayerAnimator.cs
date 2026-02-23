using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


struct PlayerAnimatorState
{
    public const string IsWalking = "IsWalking";
    
}
public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField]private Player player;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    
    private void Update()
    {
        if(!IsOwner) return;
        animator.SetBool(PlayerAnimatorState.IsWalking, player.IsWalking());
    }
}
