using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public void StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete, Action<string> onError)
        {
            string url = playerId;
            StartCoroutine(Get(url, onStartMatchComplete, onError));
        }

        private IEnumerator Get(string url, Action<MatchStatus> onStartMatchComplete, Action<string> onError)
        {
            bool isComplete;
            bool isError;
            string responseString;
            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                isError = webRequest.isNetworkError;
                isComplete = webRequest.isDone;
                responseString = isError ? webRequest.error :
                    isComplete ? webRequest.downloadHandler.text : string.Empty;
                if (isComplete)
                {
                    // if (!responseString.Contains("matchfound:")) //hardcodeo berrta.
                    //     isComplete = false;
                }
            }

            if (isError)
            {
                onError(responseString);
            }

            if (isComplete)
            {
                //var dto = JsonUtility.FromJson<MatchStatusDto>(responseString)
                //onStartMatchComplete(DtoToMatchStatus(dto));
                onStartMatchComplete(DtoToMatchStatus(new MatchStatusDto()));
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onStartMatchComplete, onError));
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
                        "Garland Wars"
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
                    UpgradeCardRound = ScriptableObject.CreateInstance<UpgradeCardData>(),
                    CardsPlayed = new List<PlayerCard>()
                }).ToList()
            };
            ms.hand = new Hand(dto.hand.Units.Select(
                        cardName => new InMemoryCardProvider().GetUnitCards()
                            .FirstOrDefault(f => f.cardName == cardName))
                    .ToList(),
                dto.hand.Upgrades.Select(cardName =>
                    new InMemoryCardProvider().GetUpgradeCards().FirstOrDefault(f => f.cardName == cardName)).ToList());
            ms.round = dto.round;
            return ms;
        }

        public void PlayUpgradeCard(string cardName, Action<Round> onUpgradeCardsFinished, Action<string> onError)
        {
            var round = new Round
            {
                WinnerPlayer = "a",
                CardsPlayed = new List<PlayerCard>
                {
                    new PlayerCard
                    {
                        Player = "a",
                        UpgradeCardData = new UpgradeCardData {cardName = "Garland Wars"}
                    },
                    new PlayerCard
                    {
                        Player = "b",
                        UpgradeCardData = new UpgradeCardData {cardName = "Garland Wars"}
                    }
                },
                UpgradeCardRound = new UpgradeCardData
                {
                    cardName = "Supremacy"
                }
            };
            onUpgradeCardsFinished(round);
        }

        public void PlayUnitCard(string cardName, Action<RoundResult> onRoundComplete, Action<string> onError)
        {
            //TODO: Change to round status Id repost.
            var roundResult = new RoundResult()
            {
                newRoundCard = new UpgradeCardData {cardName = "Supremacy"},
                previousRound = new Round
                {
                    WinnerPlayer = "a",
                    CardsPlayed = new List<PlayerCard>
                    {
                        new PlayerCard
                        {
                            Player = "a",
                            UpgradeCardData = new UpgradeCardData {cardName = "Garland Wars"}
                        },
                        new PlayerCard
                        {
                            Player = "b",
                            UpgradeCardData = new UpgradeCardData {cardName = "Garland Wars"}
                        }
                    },
                    UpgradeCardRound = new UpgradeCardData
                    {
                        cardName = "Supremacy"
                    }
                }
            };
            onRoundComplete(roundResult);
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