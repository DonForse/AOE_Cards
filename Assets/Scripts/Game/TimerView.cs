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
        [SerializeField] private TextMeshProUGUI textPhase;

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
                    textPhase.text = "Rerolling Cards";
                    break;
                case MatchState.SelectUpgrade:
                    StartTimer();
                    textPhase.text = "Select Upgrade";
                    break;
                case MatchState.RoundUpgradeReveal:
                case MatchState.StartUnit:
                    StartTimer();
                    textPhase.text = "Revealing Upgrade";
                    break;
                case MatchState.SelectUnit:
                    StartTimer();
                    textPhase.text = "Select Unit";
                    break;
                case MatchState.StartRound:
                case MatchState.StartUpgrade:
                case MatchState.StartReroll:
                case MatchState.WaitReroll:
                case MatchState.WaitUnit:
                case MatchState.WaitUpgrade:
                case MatchState.InitializeGame:
                    StopTimer();
                    textPhase.text = "Wait Opponent";
                    break;
                case MatchState.EndRound:
                case MatchState.EndGame:
                case MatchState.RoundResultReveal:
                    StopTimer();
                    textPhase.text = "Revealing Winner";
                    break;
                case MatchState.UpgradeReveal:
                case MatchState.StartRoundUpgradeReveal:
                    StopTimer();
                    textPhase.text = "Revealing Round Upgrade";
                    break;
                default:
                    textPhase.text = string.Empty;
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
