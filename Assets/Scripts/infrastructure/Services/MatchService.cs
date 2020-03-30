using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class MatchService : MonoBehaviour, IMatchService
    {
        private const string BaseUrl = "https://localhost:44324/";
        private string StartMatchUrl => BaseUrl + "/api/match?userid={0}";

        public void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<string> onError)
        {
            string url = string.Format(StartMatchUrl, playerId);
            StartCoroutine(Get(url, onStartMatchComplete, onError));
        }

        private IEnumerator Get(string url, Action<Match> onStartMatchComplete, Action<string> onError)
        {
            bool isComplete;
            bool isError;
            string responseString;
            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                isError = webRequest.responseCode >= 400 || webRequest.isNetworkError;
                isComplete = webRequest.isDone;
                responseString = isError ?
                                   webRequest.error
                                   : isComplete ? Encoding.UTF8.GetString(webRequest.downloadHandler.data, 3, webRequest.downloadHandler.data.Length - 3)
                                   : string.Empty;
                Debug.Log(responseString);
            }

            if (isError)
            {
                onError(responseString);
            }
            else if (isComplete)
            {                
                var dto = JsonUtility.FromJson<MatchDto>(responseString);
                if (dto.matchId == null)
                {
                    yield return new WaitForSeconds(3f);
                    StartCoroutine(Get(url, onStartMatchComplete, onError));
                }
                else {
                    onStartMatchComplete(DtoToMatchStatus(dto));
                }
                
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onStartMatchComplete, onError));
            }
        }

        private Match DtoToMatchStatus(MatchDto dto)
        {
            var ms = new Match();
            ms.id = dto.matchId;
            ms.board = new Board
            {
                Rounds = dto.board.rounds.Select(r =>
                    new Round
                    {
                        WinnerPlayer = r.winnerplayer,
                        UpgradeCardRound = new InMemoryCardProvider().GetUpgradeCard(r.upgradecardround),
                        CardsPlayed = r.cardsplayed?.Select(cp =>
                            new PlayerCard
                            {
                                Player = cp.player,
                                UnitCardData = new InMemoryCardProvider().GetUnitCard(cp.unitcard),
                                UpgradeCardData = new InMemoryCardProvider().GetUpgradeCard(cp.upgradecard)
                            }).ToList()
                    }).ToList()
            };
            ms.hand = new Hand(dto.hand.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).ToList(),
                        dto.hand.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).ToList());
            ms.users = dto.users;
            return ms;
        }
    }
}