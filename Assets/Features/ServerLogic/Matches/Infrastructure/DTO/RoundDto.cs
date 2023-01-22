using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Infrastructure.DTO
{
    public class RoundDto
    {
        public bool finished;
        public IList<PlayerCardDto> cardsplayed;
        public IList<string> winnerplayer;
        public string upgradecardround;
        public int roundnumber;
        public bool rivalready;
        public bool hasReroll;
        public RoundState roundState;
        public int roundTimer;

        public RoundDto(Round round, IList<User> users, string userId)
        {
            if (round == null)
                return;
            winnerplayer = round.PlayerWinner != null ? round.PlayerWinner.Select(c => c.UserName).ToList() : new List<string>();
            finished = winnerplayer.Count > 0;
            upgradecardround = round.RoundUpgradeCard?.cardName;
            roundnumber = round.roundNumber;
            hasReroll = !round.PlayerHasRerolled[userId];
            cardsplayed = new List<PlayerCardDto>();
            roundState = round.RoundState;
            roundTimer = round.Timer;
            bool showUnitCards = round.PlayerCards[userId].UnitCard != null;
            bool showUpgradeCards = round.PlayerCards[userId].UpgradeCard != null;
            var rival = round.PlayerCards.FirstOrDefault(c => c.Key != userId).Value;
            rivalready = round.RoundState == RoundState.Upgrade ? rival.UpgradeCard != null : rival.UnitCard != null;
            foreach (var playerCard in round.PlayerCards)
            {
                var pc = new PlayerCardDto()
                {
                    player = users.FirstOrDefault(u => u.Id == playerCard.Key)?.UserName,
                    upgradecard = showUpgradeCards ? playerCard.Value.UpgradeCard?.cardName : string.Empty,
                    unitcard = showUnitCards ? playerCard.Value.UnitCard?.cardName : string.Empty,
                    unitcardpower = showUnitCards ? playerCard.Value.UnitCardPower : 0,
                };
                cardsplayed.Add(pc);
            }
        }
    }
}

