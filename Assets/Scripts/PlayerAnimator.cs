using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct PlayerAnimatorState
{
    public const string IsWalking = "IsWalking";
    
}
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]private Player player;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    
    private void Update()
    {
        animator.SetBool(PlayerAnimatorState.IsWalking, player.IsWalking());
    }
}
