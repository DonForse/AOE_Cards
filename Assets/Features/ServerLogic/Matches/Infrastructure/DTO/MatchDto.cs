using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace ServerLogic.Matches.Infrastructure.DTO
{
    public class MatchDto
    {
        public string matchId;
        public BoardDto board;
        public HandDto hand;
        public List<string> users;

        public MatchDto(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId)
        {
            if (serverMatch == null)
                return;
            matchId = serverMatch.Guid;
            this.board = new BoardDto
            {
                rounds = new List<RoundDto>()
            };
            foreach (var round in serverMatch.Board.RoundsPlayed)
            {
                this.board.rounds.Add(new RoundDto(round, serverMatch.Users, userId));
            }
            hand = new HandDto();
            hand.units = serverMatch.Board.PlayersHands[userId].UnitsCards.Select(c => c.CardName).ToList();
            hand.upgrades = serverMatch.Board.PlayersHands[userId].UpgradeCards.Select(c => c.CardName).ToList();
            users = serverMatch.Users.Select(u => u.UserName).ToList();
        }
    }
}
