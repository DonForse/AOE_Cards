using System;
using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Match.Domain;
using Infrastructure;
using Infrastructure.Data;
using Match.Domain;
using ServerLogic.Controllers;
using ServerLogic.Matches.Infrastructure.DTO;
using UniRx;
using UnityEngine;
using MatchDto = Infrastructure.DTOs.MatchDto;

namespace Match
{
    public class OfflineMatchService : IMatchService
    {
        private readonly ICardProvider _cardProvider;
        private readonly MatchController _matchController;
        private string UserId() => PlayerPrefs.GetString(PlayerPrefsHelper.UserId);

        public OfflineMatchService(ICardProvider cardProvider, MatchController matchController)
        {
            _cardProvider = cardProvider;
            _matchController = matchController;
        }

        public IObservable<GameMatch> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            var response = _matchController.Post(UserId(),
                new MatchInfoDto {vsBot = vsBot, vsFriend = vsFriend, friendCode = friendCode, botDifficulty = botDifficulty});
            
            var dto = JsonUtility.FromJson<MatchDto>(response.response);
            if (string.IsNullOrWhiteSpace(dto.matchId))
                return Observable.Return((GameMatch)null);
            return Observable.Return(DtoToMatchStatus(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<GameMatch> GetMatch()
        {
            var response = _matchController.Get(UserId());
            var dto = JsonUtility.FromJson<MatchDto>(response.response);
            return Observable.Return(DtoToMatchStatus(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Unit> RemoveMatch()
        {
            _matchController.Delete(UserId());
            return Observable.ReturnUnit();
        }

        public void StopSearch() => Debug.Log("Impossible To Cancel");

        private GameMatch DtoToMatchStatus(MatchDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.matchId))
                return null;
            var ms = new GameMatch();
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
                        RoundNumber = r.roundnumber,
                        CardsPlayed = r.cardsplayed?.Select(cp =>
                            new PlayerCard
                            {
                                Player = cp.player,
                                UnitCardData = _cardProvider.GetUnitCard(cp.unitcard),
                                UpgradeCardData = _cardProvider.GetUpgradeCard(cp.upgradecard)
                            }).ToList()
                    }).ToList()
            };
            ms.Hand = new Hand(
                dto.hand.units.Select(cardName => _cardProvider.GetUnitCard(cardName))
                    .OrderByDescending(c => c.cardName.ToLower() == "villager" ? -1 : c.power).ToList(),
                dto.hand.upgrades.Select(cardName => _cardProvider.GetUpgradeCard(cardName))
                    .OrderBy(c => c.GetArchetypes().FirstOrDefault()).ToList());
            ms.Users = dto.users;
            return ms;
        }
    }
}