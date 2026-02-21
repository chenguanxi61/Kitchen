using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    
    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }
    
    private State state;
    
    public Action<State> OnStateChanged;
    
    private float waitingToStartTimer = 0.1f;
    private float countDownToStartTimer = 3f;
    private float gamePlayingTimer = 30f;
    private float gamePlayingTimerMax = 30f;
    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer <= 0f)
                {
                    state = State.CountdownToStart;
                    OnStateChanged?.Invoke(state);
                }
                break;
            case State.CountdownToStart:
                countDownToStartTimer -= Time.deltaTime;
                if (countDownToStartTimer <= 0f)
                {
                    state = State.GamePlaying;
                    OnStateChanged?.Invoke(state);
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer <= 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(state);
                }
                break;
            case State.GameOver:
                break;
        }
    }
    
    public bool IsPlaying()
    {
        return state == State.GamePlaying;
    }
    
    public float GetCountDownToStartTimer()
    {
        return countDownToStartTimer;
    }
    
    public bool IsOver()
    {
        return state == State.GameOver;
    }

    public float GetPlayingTimerNormalized()
    {
        return 1-(gamePlayingTimer/gamePlayingTimerMax);
    }
}
