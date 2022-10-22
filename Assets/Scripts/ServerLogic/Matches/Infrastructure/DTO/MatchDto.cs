using System.Collections.Generic;
using System.Linq;

namespace ServerLogic.Matches.Infrastructure.DTO
{
    public class MatchDto
    {
        public string matchId;
        public BoardDto board;
        public HandDto hand;
        public List<string> users;

        public MatchDto(Domain.Match match, string userId)
        {
            if (match == null)
                return;
            matchId = match.Guid;
            this.board = new BoardDto
            {
                rounds = new List<RoundDto>()
            };
            foreach (var round in match.Board.RoundsPlayed)
            {
                this.board.rounds.Add(new RoundDto(round, match.Users, userId));
            }
            hand = new HandDto();
            hand.units = match.Board.PlayersHands[userId].UnitsCards.Select(c => c.CardName).ToList();
            hand.upgrades = match.Board.PlayersHands[userId].UpgradeCards.Select(c => c.CardName).ToList();
            users = match.Users.Select(u => u.UserName).ToList();
        }
    }
}
