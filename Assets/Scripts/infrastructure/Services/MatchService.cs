using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class MatchService : MonoBehaviour, IMatchService
    {
        private string MatchUrl => Configuration.UrlBase + "/api/match";

        private bool stopLooking = false;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            stopLooking = false;
            string data = JsonUtility.ToJson(new MatchPostDto { vsBot = vsBot, vsFriend = vsFriend, friendCode = friendCode, botDifficulty = botDifficulty });
            return Post(data).Retry(3);
        }

        public void GetMatch(Action<Match> onStartMatchComplete, Action<long, string> onError)
        {
            stopLooking = false;
            StartCoroutine(Get(onStartMatchComplete, onError));
        }


        public IObservable<Unit> RemoveMatch()
        {
            stopLooking = true;
            return Delete().Retry(3);
        }

        private IObservable<Unit> Delete()
        {
            return Observable.Create<Unit>(emitter =>
            {
                ResponseInfo responseInfo;
                var webRequest = UnityWebRequest.Delete(MatchUrl);

                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                return webRequest.SendWebRequest().AsObservable()
                    .DoOnCompleted(() => webRequest.Dispose())
                    .Subscribe(_ =>
                    {
                        responseInfo = new ResponseInfo(webRequest);
                        if (responseInfo.isError)
                        {
                            emitter.OnError(new MatchServiceException(responseInfo.response.error, responseInfo.code));
                            emitter.OnCompleted();
                        }
                        else if (responseInfo.isComplete)
                        {
                            emitter.OnNext(Unit.Default);
                            emitter.OnCompleted();
                        }
                        else
                        {
                            throw new TimeoutException("cannot reach server");
                        }

                    }).AddTo(_disposables);
            });
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
                else
                {
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

        private IObservable<Match> Post(string data)
        {
            return Observable.Create<Match>(emitter => {
                ResponseInfo responseInfo;
                var webRequest = UnityWebRequest.Post(MatchUrl, data);

                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json;charset=ISO-8859-1");
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                return webRequest.SendWebRequest().AsObservable()
                .DoOnCompleted(() => webRequest.Dispose())
                .Subscribe(_ =>
                {
                    responseInfo = new ResponseInfo(webRequest);
                    if (responseInfo.isError)
                    {
                        emitter.OnError(new MatchServiceException(responseInfo.response.error, responseInfo.code));
                        emitter.OnCompleted();
                    }
                    else if (responseInfo.isComplete)
                    {
                        var dto = JsonUtility.FromJson<MatchDto>(responseInfo.response.response);
                        if (string.IsNullOrWhiteSpace(dto.matchId))
                        {
                            emitter.OnNext(null);
                        }
                        else
                        {
                            emitter.OnNext(DtoToMatchStatus(dto));
                            emitter.OnCompleted();
                        }
                    }
                    else
                    {
                        throw new TimeoutException("cannot reach server");
                    }
                }
                ).AddTo(_disposables);
            });
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
            ms.Hand = new Hand(dto.hand.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).OrderByDescending(c => c.cardName.ToLower() == "villager" ? -1 : c.power).ToList(),
                        dto.hand.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
            ms.Users = dto.users;
            return ms;
        }
    }
}