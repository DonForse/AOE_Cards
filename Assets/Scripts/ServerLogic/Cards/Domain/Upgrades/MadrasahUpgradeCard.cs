using System.Linq;
using ServerLogic.Matches.Domain;

namespace ServerLogic.Cards.Domain.Upgrades
{
    public class MadrasahUpgradeCard : UpgradeCard
    {
        public override void ApplicateEffectPostUnit(Matches.Domain.Match match, string userId)
        {
            var currentRound = match.Board.RoundsPlayed.Last();
            var unitPlayed = currentRound.PlayerCards[userId].UnitCard;
            if (unitPlayed.Archetypes.Any(upArchetype => upArchetype == Archetype.Monk))
                match.Board.PlayersHands[userId].UnitsCards.Add(unitPlayed);
        }
    }
}

