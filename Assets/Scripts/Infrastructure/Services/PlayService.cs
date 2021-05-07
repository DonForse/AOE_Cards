using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;

namespace Infrastructure.Services
{
    public class PlayService : MonoBehaviour, IPlayService
    {
        private string GetRoundUrl => Configuration.UrlBase + "/api/round?matchid={0}&roundNumber={1}";
        private string PlayCardUrl => Configuration.UrlBase + "/api/play?matchid={0}";
        private string RerollUrl => Configuration.UrlBase + "/api/reroll?matchid={0}";

        public IObservable<Round> GetRound(int roundNumber)
        {
            var url = string.Format(GetRoundUrl, PlayerPrefs.GetString(PlayerPrefsHelper.MatchId), roundNumber);
            return Get(url).Retry(3);
        }

        public IObservable<Hand> PlayUpgradeCard(string cardName)
        {
            string data = JsonUtility.ToJson(new CardPostDto { cardname = cardName, type = "upgrade" });
            return PlayCard(data).Retry(3);
        }

        public IObservable<Hand> PlayUnitCard(string cardName)
        {
            string data = JsonUtility.ToJson(new CardPostDto { cardname = cardName, type = "unit" });
            return PlayCard(data);
        }

        public IObservable<Hand> RerollCards(IList<string> unitCards, IList<string> upgradeCards)
        {
            string data = JsonUtility.ToJson(new RerollInfoDto { unitCards = unitCards.ToArray(), upgradeCards = upgradeCards.ToArray() });
            return RerollCards(data).Retry(3);
        }

        private IObservable<Round> Get(string url)
        {
            return Observable.Create<Round>(emitter =>
            {
                ResponseInfo responseInfo;
                var webRequest = UnityWebRequest.Get(url);

                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                return webRequest.SendWebRequest().AsObservable()
                .DoOnCompleted(() => webRequest.Dispose())
                .Subscribe(_ =>
                {
                    responseInfo = new ResponseInfo(webRequest);
                    if (responseInfo.isError)
                    {
                        emitter.OnError(new PlayServiceException(responseInfo.code, responseInfo.response?.error));
                    }
                    else if (responseInfo.isComplete)
                    {
                        var dto = JsonUtility.FromJson<RoundDto>(responseInfo.response?.response);
                        emitter.OnNext(DtoToRound(dto));
                        emitter.OnCompleted();
                        //onStartMatchComplete(DtoToMatchStatus(new MatchStatusDto()));
                    }
                    else
                    {
                        throw new TimeoutException("Server does not respond");
                    }
                });
            });
        }

        private Round DtoToRound(RoundDto dto)
        {
            return new Round
            {
                Finished = dto.finished,
                RoundNumber = dto.roundnumber,
                WinnerPlayers = dto.winnerplayer,
                UpgradeCardRound = new InMemoryCardProvider().GetUpgradeCard(dto.upgradecardround),
                CardsPlayed = dto.cardsplayed.Select(cp =>
                    new PlayerCard
                    {
                        Player = cp.player,
                        UnitCardData = new InMemoryCardProvider().GetUnitCard(cp.unitcard),
                        UpgradeCardData = new InMemoryCardProvider().GetUpgradeCard(cp.upgradecard),
                        UnitCardPower = cp.unitcardpower,
                    }).ToList(),
                RivalReady = dto.rivalready,
                RoundState = dto.roundState,
                HasReroll = dto.hasReroll,
                Timer = dto.roundTimer

            };
        }

        private IObservable<Hand> PlayCard(string data)
        {
            return Observable.Create<Hand>(emitter =>
            {
                ResponseInfo responseInfo;
                var playCardUrl = string.Format(PlayCardUrl, PlayerPrefs.GetString(PlayerPrefsHelper.MatchId));

                Debug.Log(playCardUrl);
                var webRequest = UnityWebRequest.Post(playCardUrl, data);

                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json;charset=ISO-8859-1");
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                return webRequest.SendWebRequest().AsObservable().DoOnCompleted(() => webRequest.Dispose()).Subscribe(_ =>
                {
                    responseInfo = new ResponseInfo(webRequest);
                    if (responseInfo.isError)
                    {
                        emitter.OnError(new PlayServiceException(responseInfo.code, responseInfo.response?.error));
                    }
                    else if (responseInfo.isComplete)
                    {
                        var dto = JsonUtility.FromJson<HandDto>(responseInfo.response?.response);
                        emitter.OnNext(DtoToHand(dto));
                        emitter.OnCompleted();
                    }
                    else
                    {
                        throw new TimeoutException("Request timed out");
                    }
                });
            });
        }

        private IObservable<Hand> RerollCards(string data)
        {
            return Observable.Create<Hand>(emitter =>
            {
                ResponseInfo responseInfo;
                var rerollUrl = string.Format(RerollUrl, PlayerPrefs.GetString(PlayerPrefsHelper.MatchId));

                Debug.Log(rerollUrl);
                var webRequest = UnityWebRequest.Post(rerollUrl, data);

                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json;charset=ISO-8859-1");
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));

                return webRequest.SendWebRequest().AsObservable().DoOnCompleted(() => webRequest.Dispose()).Subscribe(_ =>
                {
                    responseInfo = new ResponseInfo(webRequest);
                    if (responseInfo.isError)
                    {
                        emitter.OnError(new PlayServiceException(responseInfo.code, responseInfo.response?.error));
                    }
                    else if (responseInfo.isComplete)
                    {
                        var dto = JsonUtility.FromJson<HandDto>(responseInfo.response?.response);
                        emitter.OnNext(DtoToHand(dto));
                        emitter.OnCompleted();

                    }
                    else
                    {
                        throw new TimeoutException("Response timed out");
                    }
                });
            });
        }

        private Hand DtoToHand(HandDto dto)
        {
            return new Hand(dto.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).OrderBy(c => c.cardName.ToLower() == "villager" ? int.MaxValue : c.power).ToList(),
                        dto.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
        }
    }
}