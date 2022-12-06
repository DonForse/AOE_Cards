using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Domain
{
    public class ServerMatch
    {
        private readonly Random random = new Random();
        public string Guid;
        public Board Board;
        public IList<User> Users;
        public User MatchWinner;
        public bool IsTie;
        public bool IsFinished;
        public bool IsBot;
        public int BotDifficulty;
        
        public List<UnitCard> GetVsUnits(Round currentRound, string userId)
        {
            return currentRound.PlayerCards
                .Where(pc => pc.Key != userId)
                .Select(c => c.Value.UnitCard).ToList();
        }

        public List<UpgradeCard> GetUpgradeCardsByPlayer(Round currentRound, string userId)
        {
            var upgradeCards = Board.RoundsPlayed.SelectMany(r => r.PlayerCards
                .Where(pc => pc.Key == userId && pc.Value.UpgradeCard != null)
                .Select(pc => pc.Value.UpgradeCard)).ToList();
            upgradeCards.Add(currentRound.RoundUpgradeCard);
            return upgradeCards;
        }
    }
}

