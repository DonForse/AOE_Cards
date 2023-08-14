﻿using System.Collections.Generic;
using System.Linq;

namespace Features.ServerLogic.Matches.Infrastructure.DTO
{
    public class MatchDto
    {
        public string matchId;
        public BoardDto board;
        public HandDto hand;
        public List<string> users;

        public MatchDto(Domain.ServerMatch serverMatch, string userId)
        {
            if (serverMatch == null)
                return;
            matchId = serverMatch.Guid;
            this.board = new BoardDto
            {
                rounds = new List<RoundDto>()
            };
            var rounds =
                serverMatch.Board.RoundsPlayed.Where(x => x != null);
            foreach (var round in rounds)
            {
                this.board.rounds.Add(new RoundDto(round, serverMatch.Users, userId));
            }

            this.board.currentRound = new RoundDto(serverMatch.Board.CurrentRound, serverMatch.Users, userId);
            hand = new HandDto();
            hand.units = serverMatch.Board.PlayersHands[userId].UnitsCards.Select(c => c.cardName).ToList();
            hand.upgrades = serverMatch.Board.PlayersHands[userId].UpgradeCards.Select(c => c.cardName).ToList();
            users = serverMatch.Users.Select(u => u.UserName).ToList();
        }
    }
}
