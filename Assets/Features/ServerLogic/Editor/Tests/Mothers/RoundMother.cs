using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
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
            IList<User> withPlayersWinner = null, int withRoundNumber = 0, RoundState? withRoundState = null)
        {
            withUsers ??= new List<string>();
            withPlayersWinner ??= new List<User>();
            if (withPlayerCards == null)
            {
                withPlayerCards = new Dictionary<string, PlayerCard>();
                foreach (var user in withUsers) withPlayerCards.Add(user, new PlayerCard());
            }

            
            
            var r = new Round(withUsers)
            {
                roundNumber = withRoundNumber,
                PlayerCards = withPlayerCards,
                PlayerHasRerolled = withPlayerReroll,
                PlayerWinner = withPlayersWinner,
                RoundUpgradeCard = withRoundUpgradeCard,
                NextBotActionTimeInSeconds = withNextBotAction,
            };

            if (withRoundState != null)
                r.ChangeRoundState(withRoundState.Value);
            return r;
        }
    }
}