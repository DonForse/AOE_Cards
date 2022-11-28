using System;
using Features.Game.Scripts.Domain;
using Infrastructure.Data;
using TMPro;
using UnityEngine;

namespace Features.Game.Scripts.Delivery
{
    public class TimerView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI textTimer;
        [SerializeField] private Color defaultColor;
        [SerializeField] private Color lowTimeColor;

        private float lowtimer;
        private float currentTime;

        private bool doingCountdown = false;

        // Update is called once per frame
        void Update()
        {
            if (!doingCountdown)
                return;
            currentTime -= Time.deltaTime;

            textTimer.color = currentTime <= lowtimer ? lowTimeColor : defaultColor;

            textTimer.text = currentTime <= 0 ? "-" : ((int)Math.Floor(currentTime)).ToString();
        }

        internal void Update(Round round)
        {
            currentTime = round.Timer;
        }

        public TimerView WithLowTimer(float lowTime)
        {
            lowtimer = lowTime;
            return this;
        }
        public void ShowState(GameState state)
        {
            switch (state)
            {
                case GameState.WaitReroll:
                case GameState.WaitUnit:
                case GameState.WaitUpgrade:
                case GameState.SelectReroll:
                case GameState.SelectUpgrade:
                case GameState.StartUnit:
                case GameState.SelectUnit:
                    StartTimer();
                    break;

                case GameState.StartRound:
                case GameState.StartUpgrade:
                case GameState.StartReroll:
                case GameState.InitializeGame:
                case GameState.EndRound:
                case GameState.EndGame:
                case GameState.RoundResultReveal:
                case GameState.UpgradeReveal:
                case GameState.StartRoundUpgradeReveal:
                    StopTimer();
                    break;
                default:
                    break;
            }
        }

        public void StopTimer()
        {
            textTimer.text = "-";
            animator.SetBool("active", false);
            doingCountdown = false;
        }

        public void StartTimer()
        {
            textTimer.color = defaultColor;
            animator.SetBool("active", true);
            doingCountdown = true;
        }
    }
}
