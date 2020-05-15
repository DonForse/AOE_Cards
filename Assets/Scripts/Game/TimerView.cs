using System;
using Infrastructure.Services;
using TMPro;
using UnityEngine;

namespace Game
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
        public void ShowState(MatchState state)
        {
            switch (state)
            {
                case MatchState.Reroll:
                    StartTimer();
                    break;
                case MatchState.SelectUpgrade:
                    StartTimer();
                    break;
                case MatchState.RoundUpgradeReveal:
                case MatchState.StartUnit:
                    StartTimer();
                    break;
                case MatchState.SelectUnit:
                    StartTimer();
                    break;
                case MatchState.StartRound:
                case MatchState.StartUpgrade:
                case MatchState.StartReroll:
                case MatchState.WaitReroll:
                case MatchState.WaitUnit:
                case MatchState.WaitUpgrade:
                case MatchState.InitializeGame:
                    StopTimer();
                    break;
                case MatchState.EndRound:
                case MatchState.EndGame:
                case MatchState.RoundResultReveal:
                    StopTimer();
                    break;
                case MatchState.UpgradeReveal:
                case MatchState.StartRoundUpgradeReveal:
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
