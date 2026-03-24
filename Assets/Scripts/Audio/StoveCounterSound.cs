using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
    [SerializeField]private StoveCounter stoveCounter;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        if (stoveCounter == null)
        {
            return;
        }

        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        StoveCounter_OnStateChanged(stoveCounter.GetCurrentState());
    }

    private void OnDestroy()
    {
        if (stoveCounter != null)
        {
            stoveCounter.OnStateChanged -= StoveCounter_OnStateChanged;
        }
    }
    
    private void StoveCounter_OnStateChanged(StoveCounter.State e)
    {
        if (e == StoveCounter.State.Frying || e == StoveCounter.State.Fryed)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Pause();
        }
    }
}
