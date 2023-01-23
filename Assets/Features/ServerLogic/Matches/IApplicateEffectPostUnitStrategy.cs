using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public interface IApplicateEffectPostUnitStrategy
    {
        bool IsValid(UpgradeCard card);
        void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed);
    }
}