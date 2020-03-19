using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class MatchService : MonoBehaviour, IMatchService
    {
        private const string BaseUrl = "";
        private string StartMatchUrl => BaseUrl + "/games/users/{0}/matches";

        private string PlayTurnUrl => BaseUrl + "/games/users/{0}/matches/{1}/play/{2}";

        public void StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete)
        {
            string url = playerId;
            StartCoroutine(Get("www.google.com", onStartMatchComplete));
        }

        private IEnumerator Get(string url, Action<MatchStatus> onStartMatchComplete)
        {
            bool isComplete;
            string responseString;
            using (var www = UnityWebRequest.Get("www.google.com"))
            {
                yield return www.SendWebRequest();
                isComplete = !www.isNetworkError && www.isDone;
                responseString = isComplete ? www.downloadHandler.text : www.error;
                if (isComplete)
                {
                    if (responseString.Contains("matchfound:")) //hardcodeo berrta.
                        isComplete = true;
                }
            }

            if (isComplete)
                onStartMatchComplete(DtoToMatchStatus(JsonUtility.FromJson<MatchStatusDto>(responseString)));
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onStartMatchComplete));
            }
        }

        private MatchStatus DtoToMatchStatus(MatchStatusDto response)
        {
            var dto = new MatchStatusDto()
            {
                id = "guid",
                board = new BoardDto()
                {
                    Rounds = new List<RoundDto>()
                    {
                        new RoundDto()
                        {
                            CardsPlayed = new List<PlayerCardDto>()
                            {
                                new PlayerCardDto
                                {
                                    Player = "1",
                                    //UnitCard = "Villager",
                                    //UpgradeCard = "Supremacy"}
                                },
                                new PlayerCardDto
                                {
                                    Player = "2",
                                    //UnitCard = "Villager",
                                    //UpgradeCard = "Supremacy"}
                                },
                                new PlayerCardDto
                                {
                                    Player = "3",
                                    //UnitCard = "Villager",
                                    //UpgradeCard = "Supremacy"}
                                },
                            }
                        }
                    },
                },
                hand = new HandDto
                {
                    Units = new List<string>
                    {
                        "Paladin",
                        "Scout",
                        "Hussar",
                        "Skirmisher",
                        "Halbardier",
                        "Champion",
                        "Archer",
                    },
                    Upgrades = new List<string>
                    {
                        "Berbers Boots",
                        "Incas Villagers",
                        "Feitoria",
                        "Supremacy",
                        "Warland Wars"
                    }
                },
                round = 0
            };

            var ms = new MatchStatus();
            ms.id = dto.id;
            ms.board = new Board
            {
                Rounds = dto.board.Rounds.Select(r => new Round
                {
                    WinnerPlayer = r.WinnerPlayer,
                    UpgradeCardRound = new UpgradeCardData(),
                    CardsPlayed = new List<PlayerCard>()
                }).ToList()
            };
            ms.hand = new Hand(dto.hand.Units.Select(
                cardName => new InMemoryCardProvider().GetUnitCards().FirstOrDefault(f => f.cardName == cardName)).ToList(),
                dto.hand.Upgrades.Select(cardName =>
                    new InMemoryCardProvider().GetUpgradeCards().FirstOrDefault(f => f.cardName == cardName)).ToList());
            ms.round = dto.round;
            return ms;
        }

        public void PlayUpgradeCard(string cardName, Action<Round> onUpgradeCardsFinished)
        {
            //TODO: Change to round status Id repost.
            throw new System.NotImplementedException();
        }

        public void PlayUnitCard(string cardName, Action<RoundResult> onRoundComplete)
        {
            //TODO: Change to round status Id repost.
            throw new System.NotImplementedException();
        }
    }

    public class MatchStatusDto
    {
        public string id;
        public int round;
        public HandDto hand;
        public BoardDto board;
    }

    public class BoardDto
    {
        public IList<RoundDto> Rounds;
    }

    public class RoundDto
    {
        public IList<PlayerCardDto> CardsPlayed;
        public string WinnerPlayer;
        public string UpgradeCardRound;
    }

    public class PlayerCardDto
    {
        public string Player;
        public string UpgradeCard;
        public string UnitCard;
    }

    public class HandDto
    {
        public IList<string> Units;
        public IList<string> Upgrades;
    }
}

/* HTTP WEB REQUEST IF C# Version Errors.
 * var request = (HttpWebRequest)WebRequest.Create("http://www.example.com/recepticle.aspx");

var postData = "thing1=" + Uri.EscapeDataString("hello");
postData += "&thing2=" + Uri.EscapeDataString("world");
var data = Encoding.ASCII.GetBytes(postData);

request.Method = "POST";
request.ContentType = "application/x-www-form-urlencoded";
request.ContentLength = data.Length;

using (var stream = request.GetRequestStream())
{
stream.Write(data, 0, data.Length);
}

var response = (HttpWebResponse)request.GetResponse();

var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
 */