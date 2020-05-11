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
        private string StartMatchUrl => Configuration.UrlBase + "/api/match";

        public void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            StartCoroutine(Post(onStartMatchComplete, onError));
        }

        public void GetMatch(string playerId, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            StartCoroutine(Get(onStartMatchComplete, onError));
        }

        private IEnumerator Get(Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            ResponseInfo response;
            using (var webRequest = UnityWebRequest.Get(StartMatchUrl))
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
                var responseDto = JsonUtility.FromJson<ResponseDto>(response.response);
                var dto = JsonUtility.FromJson<MatchDto>(responseDto.response);
                if (string.IsNullOrWhiteSpace(dto.matchId))
                {
                    yield return new WaitForSeconds(3f);
                    StartCoroutine(Get(onStartMatchComplete, onError));
                }
                else {
                    onStartMatchComplete(DtoToMatchStatus(dto));
                }   
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(onStartMatchComplete, onError));
            }
        }
        private IEnumerator Post(Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            ResponseInfo response;
            using (var webRequest = UnityWebRequest.Post(StartMatchUrl,""))
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
                var responseDto = JsonUtility.FromJson<ResponseDto>(response.response);
                var dto = JsonUtility.FromJson<MatchDto>(responseDto.response);
                if (string.IsNullOrWhiteSpace(dto.matchId))
                {
                    yield return new WaitForSeconds(3f);
                    StartCoroutine(Get(onStartMatchComplete, onError));
                }
                else
                {
                    onStartMatchComplete(DtoToMatchStatus(dto));
                }
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(onStartMatchComplete, onError));
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
                        HasReroll = r.hasReroll,
                        RoundState = r.roundState,
                        CardsPlayed = r.cardsplayed?.Select(cp =>
                            new PlayerCard
                            {
                                Player = cp.player,
                                UnitCardData = new InMemoryCardProvider().GetUnitCard(cp.unitcard),
                                UpgradeCardData = new InMemoryCardProvider().GetUpgradeCard(cp.upgradecard)
                            }).ToList()
                    }).ToList()
            };
            ms.Hand = new Hand(dto.hand.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).OrderByDescending(c=>c.cardName.ToLower() == "villager" ? -1 : c.power).ToList(),
                        dto.hand.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
            ms.Users = dto.users;
            return ms;
        }
    }
}