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

    private IList<PlayerType> roundWinners = new List<PlayerType>();
    // Start is called before the first frame update

    public void SetGame(Match match) {
        SetPlayerName(PlayerPrefs.GetString(PlayerPrefsHelper.UserName), PlayerType.Player);
        SetPlayerName(match.users.FirstOrDefault(c => c != PlayerPrefs.GetString(PlayerPrefsHelper.UserName)), PlayerType.Rival);
        SetRoundNumber(match.board.Rounds.Count());
    }

    public void SetPlayerName(string name, PlayerType playerType)
    {
        var playerTxt = playerType == PlayerType.Player ? playerNameTxt : rivalNameTxt;
        playerTxt.text = name;
    }

    public void SetRoundNumber(int round)
    {
        roundTxt.text = string.Format("Round {0}", round);
    }

    public void WinRound(PlayerType playerType)
    {
        var container = playerType == PlayerType.Player ? roundWinsPlayer : roundWinsRival;
        var countWinner = roundWinners.Count(r => r == playerType);
        if (countWinner > 4)
            return;
        var images = container.GetComponentsInChildren<Image>();
        images[countWinner].color = Color.green;
        roundWinners.Add(playerType);

    }

    internal void SetPlayerName(object p)
    {
        throw new NotImplementedException();
    }
}
