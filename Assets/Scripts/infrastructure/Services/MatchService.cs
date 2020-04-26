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
        private string StartMatchUrl => Configuration.UrlBase + "/api/match?userid={0}";

        public void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            string url = string.Format(StartMatchUrl, playerId);
            StartCoroutine(Get(url, onStartMatchComplete, onError));
        }

        private IEnumerator Get(string url, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            ResponseInfo response;
            using (var webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                yield return webRequest.SendWebRequest();
                response = new ResponseInfo(webRequest);
                Debug.Log(response.response);
            }

            if (response.isError)
            {
                onError(response.code, response.response);
            }
            else if (response.isComplete)
            {                
                var dto = JsonUtility.FromJson<MatchDto>(response.response);
                if (string.IsNullOrWhiteSpace(dto.matchId))
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
            ms.Id = dto.matchId;
            ms.Board = new Board
            {
                Rounds = dto.board.rounds.Select(r =>
                    new Round
                    {
                        Finished = r.finished,
                        WinnerPlayers = r.winnerplayer,
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
            ms.Hand = new Hand(dto.hand.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).OrderBy(c=>c.cardName.ToLower() == "villager" ? int.MaxValue : c.power).ToList(),
                        dto.hand.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).OrderBy(c => c.archetypes.FirstOrDefault()).ToList());
            ms.Users = dto.users;
            return ms;
        }
    }
}