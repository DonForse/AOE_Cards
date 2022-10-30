using System;
using System.Collections.Generic;
using System.Linq;
using AoeCards.Controllers;
using Game;
using Infrastructure.Data;
using ServerLogic.Controllers;
using ServerLogic.Matches.Infrastructure.DTO;
using UniRx;
using UnityEngine;
using HandDto = Infrastructure.DTOs.HandDto;
using RoundDto = Infrastructure.DTOs.RoundDto;

namespace Infrastructure.Services
{
    public class OfflinePlayService : IPlayService
    {
        private readonly RoundController _roundController;
        private readonly PlayController _playController;
        private readonly ICardProvider _cardProvider;
        private readonly RerollController _rerollController;
        private string UserId() => PlayerPrefs.GetString(PlayerPrefsHelper.UserId);
        private string MatchId() => PlayerPrefs.GetString(PlayerPrefsHelper.MatchId);

        public OfflinePlayService(RoundController roundController, PlayController playController, ICardProvider cardProvider, RerollController rerollController)
        {
            _roundController = roundController;
            _playController = playController;
            _cardProvider = cardProvider;
            _rerollController = rerollController;
        }

        public IObservable<Round> GetRound(int roundNumber)
        {
            var responseInfo = _roundController.Get(UserId(), MatchId(), roundNumber);
            var dto = JsonUtility.FromJson<RoundDto>(responseInfo.response);
            return Observable.Return(DtoToRound(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> PlayUnitCard(string cardName)
        {
            var responseInfo = _playController.Post(UserId(), MatchId(), new CardInfoDto(){cardname = cardName, type = "unit"});
            var dto = JsonUtility.FromJson<HandDto>(responseInfo.response);
            return Observable.Return(DtoToHand(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> PlayUpgradeCard(string cardName)
        {
            var responseInfo = _playController.Post(UserId(), MatchId(), new CardInfoDto(){cardname = cardName, type = "upgrade"});
            var dto = JsonUtility.FromJson<HandDto>(responseInfo.response);
            return Observable.Return(DtoToHand(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> RerollCards(IList<string> unitCards, IList<string> upgradeCards)
        {
            var responseInfo = _rerollController.Post(UserId(), MatchId(),
                new ServerLogic.Matches.Infrastructure.DTO.RerollInfoDto {unitCards = unitCards.ToArray(), upgradeCards = upgradeCards.ToArray()});
            var dto = JsonUtility.FromJson<HandDto>(responseInfo.response);
            return Observable.Return(DtoToHand(dto)).Delay(TimeSpan.FromSeconds(1));
        }
        
        private Hand DtoToHand(HandDto dto)
        {
            return new Hand(dto.units.Select(cardName => _cardProvider.GetUnitCard(cardName)).OrderBy(c => c.cardName.ToLower() == "villager" ? int.MaxValue : c.power).ToList(),
                dto.upgrades.Select(cardName => _cardProvider.GetUpgradeCard(cardName)).OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
        }

        private Round DtoToRound(RoundDto dto)
        {
            return new Round
            {
                Finished = dto.finished,
                RoundNumber = dto.roundnumber,
                WinnerPlayers = dto.winnerplayer,
                UpgradeCardRound = _cardProvider.GetUpgradeCard(dto.upgradecardround),
                CardsPlayed = dto.cardsplayed.Select(cp =>
                    new PlayerCard
                    {
                        Player = cp.player,
                        UnitCardData = _cardProvider.GetUnitCard(cp.unitcard),
                        UpgradeCardData = _cardProvider.GetUpgradeCard(cp.upgradecard),
                        UnitCardPower = cp.unitcardpower,
                    }).ToList(),
                RivalReady = dto.rivalready,
                RoundState = dto.roundState,
                HasReroll = dto.hasReroll,
                Timer = dto.roundTimer

            };
        }
    }
}