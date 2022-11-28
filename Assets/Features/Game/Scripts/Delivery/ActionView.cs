using Features.Game.Scripts.Domain;
using TMPro;
using UnityEngine;

namespace Features.Game.Scripts.Delivery
{
    public class ActionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textPhase;

        public void ShowState(MatchState state)
        {
            switch (state)
            {
                case MatchState.SelectReroll:
                    textPhase.text = "Rerolling Cards";
                    break;
                case MatchState.SelectUpgrade:
                    textPhase.text = "Select Upgrade";
                    break;
                case MatchState.UpgradeReveal:
                case MatchState.StartUnit:
                    textPhase.text = "Revealing Upgrade";
                    break;
                case MatchState.SelectUnit:
                    textPhase.text = "Select Unit";
                    break;
                case MatchState.StartRound:
                case MatchState.StartUpgrade:
                case MatchState.StartReroll:
                case MatchState.WaitReroll:
                case MatchState.WaitUnit:
                case MatchState.WaitUpgrade:
                case MatchState.InitializeGame:
                    textPhase.text = "Wait Opponent";
                    break;
                case MatchState.EndRound:
                case MatchState.EndGame:
                case MatchState.RoundResultReveal:
                    textPhase.text = "Revealing Winner";
                    break;
                case MatchState.StartRoundUpgradeReveal:
                    textPhase.text = "Revealing Round Upgrade";
                    break;
                default:
                    textPhase.text = string.Empty;
                    break;
            }
        }
    }
}
