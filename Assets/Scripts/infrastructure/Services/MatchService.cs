using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class MatchService : MonoBehaviour, IMatchService
    {
        [SerializeField] InMemoryCardProvider _cardProvider;
        private string MatchUrl => Configuration.UrlBase + "/api/match";

        private CompositeDisposable _disposables = new CompositeDisposable();
        private UnityWebRequest _getWebRequest;
        private UnityWebRequest _deleteWebRequest;
        private UnityWebRequest _postWebRequest;

        public void StopSearch()
        {
            _disposables.Clear();
        }

        public IObservable<Match> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            string data = JsonUtility.ToJson(new MatchPostDto { vsBot = vsBot, vsFriend = vsFriend, friendCode = friendCode, botDifficulty = botDifficulty });
            return Post(data).Retry(3);
        }

        public IObservable<Match> GetMatch()
        {
            return Get().Retry(3);
        }


        public IObservable<Unit> RemoveMatch()
        {
            return Delete().Retry(3);
        }

        private IObservable<Unit> Delete()
        {
            return Observable.Create<Unit>(emitter =>
            {
                ResponseInfo responseInfo;
                _deleteWebRequest = UnityWebRequest.Delete(MatchUrl);

                _deleteWebRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                return _deleteWebRequest.SendWebRequest().AsObservable()
                    .DoOnCompleted(() => _deleteWebRequest?.Dispose())
                    .Subscribe(_ =>
                    {
                        responseInfo = new ResponseInfo(_deleteWebRequest);
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
                            throw new TimeoutException("Cannot reach server");
                        }

                    });
            });
        }

        private IObservable<Match> Get()
        {
            return Observable.Create<Match>(emitter =>
            {
                ResponseInfo responseInfo;
                var webRequest = UnityWebRequest.Get(MatchUrl);

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
                            throw new TimeoutException("Cannot reach server");
                        }
                    }).AddTo(_disposables);
            });
        }

private IObservable<Match> Post(string data)
{
    return Observable.Create<Match>(emitter =>
    {
        ResponseInfo responseInfo;
        _postWebRequest = UnityWebRequest.Post(MatchUrl, data);

        byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
        _postWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        _postWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        _postWebRequest.method = UnityWebRequest.kHttpVerbPOST;
        _postWebRequest.SetRequestHeader("Content-Type", "application/json;charset=ISO-8859-1");
        _postWebRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
        return _postWebRequest.SendWebRequest().AsObservable()
        .DoOnCompleted(() => _postWebRequest?.Dispose())
        .Subscribe(_ =>
        {
            responseInfo = new ResponseInfo(_postWebRequest);
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
                throw new TimeoutException("Cannot reach server");
            }
        });
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
                UpgradeCardRound = _cardProvider.GetUpgradeCard(r.upgradecardround),
                HasReroll = r.hasReroll,
                Timer = r.roundTimer,
                RoundState = r.roundState,
                CardsPlayed = r.cardsplayed?.Select(cp =>
                    new PlayerCard
                    {
                        Player = cp.player,
                        UnitCardData = _cardProvider.GetUnitCard(cp.unitcard),
                        UpgradeCardData = _cardProvider.GetUpgradeCard(cp.upgradecard)
                    }).ToList()
            }).ToList()
    };
    ms.Hand = new Hand(dto.hand.units.Select(cardName => _cardProvider.GetUnitCard(cardName)).OrderByDescending(c => c.cardName.ToLower() == "villager" ? -1 : c.power).ToList(),
                dto.hand.upgrades.Select(cardName => _cardProvider.GetUpgradeCard(cardName)).OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
    ms.Users = dto.users;
    return ms;
}
    }
}