using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public interface IApplicateEffectPostUnitStrategy
    {
        bool IsValid(UpgradeCard card);
        void Execute(ServerMatch serverMatch, string userId, UnitCard unitCardPlayed);
    }
}