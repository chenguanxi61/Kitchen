using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClipSO soundSO;
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        //送餐
        DeliverManager.Instance.OnDeliverySuccess += DeliverManager_OnDeliverySuccess;
        DeliverManager.Instance.OnDeliveryFail += DeliverManager_OnDeliveryFail;
        //切菜
        CuttingCounter.OnCutting += CuttingCounter_OnCutting;
        //拿取东西
        //Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        //玩家移动
        //Player.Instance.OnMoving += Player_OnMoving;
        //物品放置
        BaseCounter.OnAnyObjectPlaced += BaseCounter_OnAnyObjectPlaced;
        //扔垃圾
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
        
        
    }

    public void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }
    
    public void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }
    
    private void DeliverManager_OnDeliverySuccess()
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(soundSO.deliverySuccess, deliveryCounter.transform.position);
    }
    
    private void DeliverManager_OnDeliveryFail()
    {
        DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
        PlaySound(soundSO.deliveryFail, deliveryCounter.transform.position);
    }
    private void CuttingCounter_OnCutting(Vector3 position)
    {
        PlaySound(soundSO.chop,position);
    }
    
    private void Player_OnPickedSomething(Vector3 position)
    {
        PlaySound(soundSO.pickUp,position);
    }
    
    private void Player_OnMoving(Vector3 position)
    {
        //PlaySound(soundSO.footSteps,position);
    }
    
    private void BaseCounter_OnAnyObjectPlaced(Vector3 position)
    {
        PlaySound(soundSO.objectDrop,position);
    }
    
    private void TrashCounter_OnAnyObjectTrashed(Vector3 position)
    {
        PlaySound(soundSO.trash,position);
    }
    
    
}
