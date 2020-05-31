using System;
using System.Collections;
using System.Linq;
using System.Text;
using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class MatchService : MonoBehaviour, IMatchService
    {
        private string MatchUrl => Configuration.UrlBase + "/api/match";

        private bool stopLooking = false;

        public void StartMatch(bool vsBot, bool vsFriend,string friendCode, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            stopLooking = false;
            string data = JsonUtility.ToJson(new MatchPostDto { vsBot = vsBot, vsFriend = vsFriend, friendCode = friendCode });
            StartCoroutine(Post(data, onStartMatchComplete, onError));
        }

        public void GetMatch(Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            stopLooking = false;
            StartCoroutine(Get(onStartMatchComplete, onError));
        }


        public void RemoveMatch(Action onRemoveMatchComplete, Action<long, string> onError)
        {
            stopLooking = true;
            StartCoroutine(Delete(onRemoveMatchComplete, onError));
        }

        private IEnumerator Delete(Action onRemoveMatchComplete, Action<long, string> onError)
        {
            ResponseInfo responseInfo;
            using (var webRequest = UnityWebRequest.Delete(MatchUrl))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }

            if (responseInfo.isError)
            {
                onError(responseInfo.code, responseInfo.response.error);
            }
            else if (responseInfo.isComplete)
            {
                onRemoveMatchComplete();
            }
            else 
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Delete(onRemoveMatchComplete, onError));
            }
        }

        private IEnumerator Get(Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            ResponseInfo responseInfo;
            using (var webRequest = UnityWebRequest.Get(MatchUrl))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }

            if (responseInfo.isError)
            {
                onError(responseInfo.code, responseInfo.response.error);
            }
            else if (responseInfo.isComplete)
            {
                var dto = JsonUtility.FromJson<MatchDto>(responseInfo.response.response);
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
                if (stopLooking)
                    yield break;

                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(onStartMatchComplete, onError));
            }
        }

        private IEnumerator Post(string data, Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            if (stopLooking)
                yield break;
            ResponseInfo responseInfo;
            using (var webRequest = UnityWebRequest.Post(MatchUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json;charset=ISO-8859-1");
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }

            if (responseInfo.isError)
            {
                onError(responseInfo.code, responseInfo.response.error);
            }
            else if (responseInfo.isComplete)
            {
                var dto = JsonUtility.FromJson<MatchDto>(responseInfo.response.response);
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
                        Timer = r.roundTimer,
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