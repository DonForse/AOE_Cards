using System;
using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Infrastructure;
using Features.Infrastructure.Data;
using Features.Match.Domain;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using UniRx;
using UnityEngine;
using MatchDto = Features.Infrastructure.DTOs.MatchDto;

namespace Features.Match
{
    public class OfflineMatchService : IMatchService
    {
        private readonly ICardProvider _cardProvider;
        private readonly MatchHandler _matchHandler;
        private string UserId() => PlayerPrefs.GetString(PlayerPrefsHelper.UserId);

        public OfflineMatchService(ICardProvider cardProvider, MatchHandler matchHandler)
        {
            _cardProvider = cardProvider;
            _matchHandler = matchHandler;
        }

        public IObservable<GameMatch> StartMatch(bool vsBot, bool vsFriend, string friendCode, int botDifficulty)
        {
            var response = _matchHandler.Post(UserId(),
                new MatchInfoDto {vsBot = vsBot, vsFriend = vsFriend, friendCode = friendCode, botDifficulty = botDifficulty});
            
            var dto = JsonUtility.FromJson<MatchDto>(response.response);
            if (string.IsNullOrWhiteSpace(dto.matchId))
                return Observable.Return((GameMatch)null);
            return Observable.Return(DtoToMatchStatus(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<GameMatch> GetMatch()
        {
            var response = _matchHandler.Get(UserId());
            var dto = JsonUtility.FromJson<MatchDto>(response.response);
            return Observable.Return(DtoToMatchStatus(dto)).Delay(TimeSpan.FromSeconds(1));
        }

        public IObservable<Unit> RemoveMatch()
        {
            _matchHandler.Delete(UserId());
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