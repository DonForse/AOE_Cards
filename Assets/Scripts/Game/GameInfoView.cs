using Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundTxt;
    [SerializeField] private TextMeshProUGUI playerNameTxt;
    [SerializeField] private TextMeshProUGUI rivalNameTxt;
    [SerializeField] private GameObject roundWinsPlayer;
    [SerializeField] private GameObject roundWinsRival;
    private int currentRound;
    private IList<PlayerType> roundWinners = new List<PlayerType>();

    public void SetGame(Match match)
    {
        SetPlayerName(PlayerPrefs.GetString(PlayerPrefsHelper.UserName), PlayerType.Player);
        SetPlayerName(match.Users.FirstOrDefault(c => c != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)), PlayerType.Rival);
        SetRoundNumber(match.Board.Rounds.Count());
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
        foreach (var winnerPlayer in winnerPlayers)
        {
            var playerType = PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == winnerPlayer ? PlayerType.Player : PlayerType.Rival;
            var container = playerType == PlayerType.Player ? roundWinsPlayer : roundWinsRival;
            var countWinner = roundWinners.Count(r => r == playerType);
            if (countWinner > 4)
                return;
            var images = container.GetComponentsInChildren<Image>();
            images[countWinner].color = Color.green;
            roundWinners.Add(playerType);
        }
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
        SetPlayerName(string.Empty, PlayerType.Player);
        SetPlayerName(string.Empty, PlayerType.Rival);
        foreach (var imageCounter in roundWinsPlayer.GetComponentsInChildren<Image>())
        {
            imageCounter.color = Color.white;
        }

        foreach (var imageCounter in roundWinsRival.GetComponentsInChildren<Image>())
        {
            imageCounter.color = Color.white;
        }
        SetRoundNumber(0);
    }
}
