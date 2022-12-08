using System;
using System.Collections.Generic;
using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Infrastructure.Data;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using UniRx;
using UnityEngine;
using HandDto = Features.Infrastructure.DTOs.HandDto;
using RoundDto = Features.Infrastructure.DTOs.RoundDto;

namespace Features.Infrastructure.Services
{
    public class OfflinePlayService : IPlayService
    {
        private readonly RoundHandler _roundHandler;
        private readonly PlayHandler _playHandler;
        private readonly ICardProvider _cardProvider;
        private readonly RerollHandler _rerollHandler;
        private string UserId() => PlayerPrefs.GetString(PlayerPrefsHelper.UserId);
        private string MatchId() => PlayerPrefs.GetString(PlayerPrefsHelper.MatchId);

        public OfflinePlayService(RoundHandler roundHandler, PlayHandler playHandler, ICardProvider cardProvider, RerollHandler rerollHandler)
        {
            _roundHandler = roundHandler;
            _playHandler = playHandler;
            _cardProvider = cardProvider;
            _rerollHandler = rerollHandler;
        }

        public IObservable<Round> GetRound(int roundNumber)
        {
            var responseInfo = _roundHandler.Get(UserId(), MatchId(), roundNumber);
            var dto = JsonUtility.FromJson<RoundDto>(responseInfo.response);
            return Observable.Return(DtoToRound(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> PlayUnitCard(string cardName)
        {
            var responseInfo = _playHandler.Post(UserId(), MatchId(), new CardInfoDto(){cardname = cardName, type = "unit"});
            var dto = JsonUtility.FromJson<HandDto>(responseInfo.response);
            return Observable.Return(DtoToHand(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> PlayUpgradeCard(string cardName)
        {
            var responseInfo = _playHandler.Post(UserId(), MatchId(), new CardInfoDto(){cardname = cardName, type = "upgrade"});
            var dto = JsonUtility.FromJson<HandDto>(responseInfo.response);
            return Observable.Return(DtoToHand(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Hand> ReRollCards(IList<string> unitCards, IList<string> upgradeCards)
        {
            var responseInfo = _rerollHandler.Post(UserId(), MatchId(),
                new RerollInfoDto {unitCards = unitCards.ToArray(), upgradeCards = upgradeCards.ToArray()});
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