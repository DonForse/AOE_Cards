using System.Collections.Generic;
using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Match.Domain;
using Infrastructure.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Features.Game.Scripts.Delivery
{
    public class GameInfoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roundTxt;
        [SerializeField] private TextMeshProUGUI playerNameTxt;
        [SerializeField] private TextMeshProUGUI rivalNameTxt;
        [SerializeField] private GameObject roundWins;

        [SerializeField] private Color _neutralColorCounter;
        [SerializeField] private Color _winColorCounter;
        [SerializeField] private Color _loseColorCounter;
        [SerializeField] private Color _tieColorCounter;

        private int currentRound;
        private IList<PlayerType> roundWinners = new List<PlayerType>();

        public void SetGame(GameMatch gameMatch)
        {
            Clear();
            SetPlayerName(PlayerPrefs.GetString(PlayerPrefsHelper.UserName), PlayerType.Player);
            SetPlayerName(gameMatch.Users.FirstOrDefault(c => c != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)), PlayerType.Rival);
            SetRoundWinners(gameMatch.Board.Rounds);
            SetRoundNumber(gameMatch.Board.Rounds.Count());
        }

        private void SetRoundWinners(List<Round> rounds)
        {
            foreach (var round in rounds)
            {
                if (round.RoundState == RoundState.Finished || round.RoundState == RoundState.GameFinished)
                {
                    WinRound(round.WinnerPlayers);
                }
            }
        }

        public void SetPlayerName(string name, PlayerType playerType)
        {
            var playerTxt = playerType == PlayerType.Player ? playerNameTxt : rivalNameTxt;
            playerTxt.text = name;
        }

        public void SetRoundNumber(int round)
        {
            currentRound = round;
            roundTxt.text = string.Format("Round {0}", round);
        }

        public void WinRound(IList<string> winnerPlayers)
        {
            if (winnerPlayers.Count == 0)
                return;

            var images = roundWins.GetComponentsInChildren<Image>();
            if (winnerPlayers.Count > 1)
            {
                foreach (var winnerPlayer in winnerPlayers)
                {
                    var playerType = PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == winnerPlayer ? PlayerType.Player : PlayerType.Rival;
                    var countWinner = roundWinners.Count(r => r == playerType);
                    if (countWinner > 4)
                        return;
                    roundWinners.Add(playerType);
                }
                images[currentRound - 1].color = _tieColorCounter;
                SetRoundNumber(currentRound + 1);
                return;
            }
            var winner = PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == winnerPlayers[0] ? PlayerType.Player : PlayerType.Rival;
            if (roundWinners.Count(r => r == winner) > 4)
                return;
            roundWinners.Add(winner);

            var color = winner == PlayerType.Player ? _winColorCounter: _loseColorCounter;
            images[currentRound - 1].color = color;
            SetRoundNumber(currentRound + 1);
        }

        internal MatchResult GetWinnerPlayer()
        {
            var winnersGroups = roundWinners.GroupBy(r => r).Where(group => group.Count() >= 4);
            var countWinners = winnersGroups.Count();
            if (countWinners == 0)
                return MatchResult.NotFinished;
            if (winnersGroups.Count() > 1)
                return MatchResult.Tie;
            return winnersGroups.First().Key == PlayerType.Player ? MatchResult.Win : MatchResult.Lose;
        }

        internal void Clear()
        {
            roundWinners.Clear();
            SetPlayerName(string.Empty, PlayerType.Player);
            SetPlayerName(string.Empty, PlayerType.Rival);
            foreach (var imageCounter in roundWins.GetComponentsInChildren<Image>())
            {
                imageCounter.color = _neutralColorCounter;
            }
            SetRoundNumber(1);
        }
    }
}
