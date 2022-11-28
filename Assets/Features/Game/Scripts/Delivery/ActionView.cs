using Features.Game.Scripts.Domain;
using TMPro;
using UnityEngine;

namespace Features.Game.Scripts.Delivery
{
    public class ActionView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textPhase;

        public void ShowState(GameState state)
        {
            switch (state)
            {
                case GameState.SelectReroll:
                    textPhase.text = "Rerolling Cards";
                    break;
                case GameState.SelectUpgrade:
                    textPhase.text = "Select Upgrade";
                    break;
                case GameState.UpgradeReveal:
                case GameState.StartUnit:
                    textPhase.text = "Revealing Upgrade";
                    break;
                case GameState.SelectUnit:
                    textPhase.text = "Select Unit";
                    break;
                case GameState.StartRound:
                case GameState.StartUpgrade:
                case GameState.StartReroll:
                case GameState.WaitReroll:
                case GameState.WaitUnit:
                case GameState.WaitUpgrade:
                case GameState.InitializeGame:
                    textPhase.text = "Wait Opponent";
                    break;
                case GameState.EndRound:
                case GameState.EndGame:
                case GameState.RoundResultReveal:
                    textPhase.text = "Revealing Winner";
                    break;
                case GameState.StartRoundUpgradeReveal:
                    textPhase.text = "Revealing Round Upgrade";
                    break;
                default:
                    textPhase.text = string.Empty;
                    break;
            }
        }
    }
}
