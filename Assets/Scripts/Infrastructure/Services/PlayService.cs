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
    public partial class PlayService : MonoBehaviour, IPlayService
    {
        private string GetRoundUrl => Configuration.UrlBase + "/api/play?matchid={0}&roundNumber={1}";
        private string PlayCardUrl => Configuration.UrlBase + "/api/play?matchid={0}";

        public void GetRound(int roundNumber, Action<Round> onGetRoundComplete, Action<long, string> onError)
        {
            var url = string.Format(GetRoundUrl, PlayerPrefs.GetString(PlayerPrefsHelper.MatchId), roundNumber);
            StartCoroutine(Get(url, onGetRoundComplete, onError));
        }

        public void PlayUpgradeCard(string cardName, Action<Hand> onUpgradeCardsFinished, Action<long, string> onError)
        {
            string data = JsonUtility.ToJson(new CardPostDto { cardname = cardName, type = "upgrade" });
            StartCoroutine(PlayCard(data, onUpgradeCardsFinished, onError));
        }

        public void PlayUnitCard(string cardName, Action<Hand> onUnitCardFinished, Action<long, string> onError)
        {
            string data = JsonUtility.ToJson(new CardPostDto { cardname = cardName, type = "unit" });
            StartCoroutine(PlayCard(data, onUnitCardFinished, onError));
        }

        private IEnumerator Get(string url, Action<Round> onStartMatchComplete, Action<long, string> onError)
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
                var dto = JsonUtility.FromJson<RoundDto>(response.response);
                onStartMatchComplete(DtoToRound(dto));
                //onStartMatchComplete(DtoToMatchStatus(new MatchStatusDto()));
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onStartMatchComplete, onError));
            }
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
                RivalReady = dto.rivalready
            };
        }

        private IEnumerator PlayCard(string data, Action<Hand> onPostComplete, Action<long, string> onPostFailed)
        {
            ResponseInfo response;
            var playCardUrl = string.Format(PlayCardUrl, PlayerPrefs.GetString(PlayerPrefsHelper.MatchId));

            Debug.Log(playCardUrl);
            using (var webRequest = UnityWebRequest.Post(playCardUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.AccessToken));
                yield return webRequest.SendWebRequest();
                response = new ResponseInfo(webRequest);
                Debug.Log(response.response);
            }

             if (response.isError)
            {
                onPostFailed(response.code, response.response);
            }
            else if (response.isComplete)
            {
                var dto = JsonUtility.FromJson<HandDto>(response.response);
                onPostComplete(DtoToHand(dto));

            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(PlayCard(data, onPostComplete, onPostFailed));
            }
        }

        private Hand DtoToHand(HandDto dto)
        {
            return new Hand(dto.units.Select(cardName => new InMemoryCardProvider().GetUnitCard(cardName)).OrderBy(c => c.cardName.ToLower() == "villager" ? int.MaxValue : c.power).ToList(),
                        dto.upgrades.Select(cardName => new InMemoryCardProvider().GetUpgradeCard(cardName)).OrderBy(c => c.archetypes.FirstOrDefault()).ToList());
        }
    }
}