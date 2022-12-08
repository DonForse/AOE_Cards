using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class RoundMother
    {
        public static Round Create(IEnumerable<string> withUsers = null,
            IDictionary<string, PlayerCard> withPlayerCards = null,
            IDictionary<string, bool> withPlayerReroll = null, UpgradeCard withRoundUpgradeCard = null,
            int withNextBotAction = 0,
            IList<User> withPlayersWinner = null, int withRoundNumber = 0)
        {
            return new Round(withUsers)
            {
                roundNumber = withRoundNumber,
                PlayerCards = withPlayerCards,
                PlayerReroll = withPlayerReroll,
                PlayerWinner = withPlayersWinner,
                RoundUpgradeCard = withRoundUpgradeCard,
                NextBotActionTimeInSeconds = withNextBotAction,
            };
        }
    }
}