using System.Linq;
using ServerLogic.Matches.Domain;

namespace ServerLogic.Cards.Domain.Upgrades
{
    public class TeutonsFaithUpgradeCard : UpgradeCard
    {        public override void ApplicateEffectPreCalculus(Matches.Domain.Match match, string userId)
        {
            var currentRound = match.Board.RoundsPlayed.Last();
            if (currentRound.PlayerCards[userId].UnitCard.Archetypes.All(a => a != Archetype.Cavalry))
            {
                this.BasePower = 0;
                return;
            }
            var someonePlayMonk = currentRound.PlayerCards.Where(p=>p.Key != userId)
                .Select(pc=>pc.Value.UnitCard)
                .Any(ucp => ucp.Archetypes
                    .Any(archetype => archetype == Archetype.Monk));
            if (someonePlayMonk)
                this.BasePower = 1000;// do not use because of overflow: int.MaxValue;
            else
                this.BasePower = 0;

        }
    }
}

