using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClipSO soundSO;
    
    public void Start()
    {
        DeliverManager.Instance.OnDeliverySuccess += DeliverManager_OnDeliverySuccess;
        DeliverManager.Instance.OnDeliveryFail += DeliverManager_OnDeliveryFail;
        CuttingCounter.OnCutting += CuttingCounter_OnCutting;
        Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        
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
}
