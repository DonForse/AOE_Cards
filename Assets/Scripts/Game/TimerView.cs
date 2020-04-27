using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI textTimer;
    [SerializeField] private TextMeshProUGUI textPhase;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color lowTimeColor;

    private float timer;
    private float lowtimer;
    private float currentTime;

    private Action timerCompleteCallback;
    private bool doingCountdown  =false;

    // Update is called once per frame
    void Update()
    {
        if (!doingCountdown)
            return;
        currentTime -= Time.deltaTime;
        textTimer.text = ((int)Math.Floor(currentTime)).ToString();

        if (currentTime <= lowtimer)
            textTimer.color = Color.red;
        
        if (currentTime >= 0)
            return;

        if (timerCompleteCallback == null)
            return;
        StopTimer();
        timerCompleteCallback();
    }

    public TimerView WithTimerCompleteCallback(Action timerComplete) {
        timerCompleteCallback = timerComplete;
        return this;
    }
    public TimerView WithTimer(float timerCount, float lowTime) {
        timer = timerCount;
        lowtimer = lowTime;
        currentTime = timer;
        return this;
    }
    public void ShowState(MatchState state) {
        switch (state)
        {
            case MatchState.SelectUpgrade:
                textPhase.text = "Select Upgrade";
                break;
            case MatchState.SelectUnit:
                textPhase.text = "Select Unit";
                break;
            case MatchState.WaitUnit:
            case MatchState.WaitUpgrade:
                textPhase.text = "Wait Opponent";
                break;
            case MatchState.RoundResultReveal:
                StopTimer();
                textPhase.text = "Revealing Winner";
                break;
            default:
                textPhase.text = string.Empty;
                break;
        }
    }

    public void ResetTimer()
    {
        currentTime = timer;
        textTimer.color = defaultColor;
    }

    internal object WithTimer(object turnTimer)
    {
        throw new NotImplementedException();
    }

    public void StopTimer() {
        textTimer.text = "-";
        doingCountdown = false;
    }
    public void StartCountdown()
    {
        currentTime = timer;
        textTimer.color = defaultColor;
        doingCountdown = true;
        
    }
}
