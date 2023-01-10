using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public interface IApplicateEffectPostUnitStrategy
    {
        bool IsValid(UpgradeCard card);
        void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed);
    }
}