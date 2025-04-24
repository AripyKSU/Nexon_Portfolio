using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Nostal.Interfaces;

public class Door : NetworkBehaviour, IInteractable
{
    public Animator animator;
    public Collider doorCollider;
    [Networked] public bool isOpen { get; set; } = false;
    
    public void Awake() {
        animator = transform.GetComponent<Animator>();
        doorCollider = GetComponent<Collider>();
    }
    
    //상호작용 시 문을 열고 닫음
    public virtual void OnInteract(NetworkObject playerObject)
    {
        if (isOpen) 
        {
            PlayAnimationBackwardRpc();
        }
        else 
        {
            PlayAnimationForwardRpc();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void PlayAnimationForwardRpc()
    {
        isOpen = true;
        animator.SetFloat("Speed", 1.0f);
        animator.Play("DoorAnimation", 0, 0f);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void PlayAnimationBackwardRpc()
    {
        isOpen = false;
        animator.SetFloat("Speed", -1.0f);
        animator.Play("DoorAnimation", 0, 1f);
    }
}
