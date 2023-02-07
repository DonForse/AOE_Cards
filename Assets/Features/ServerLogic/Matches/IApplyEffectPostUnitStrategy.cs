using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Matches
{
    public interface IApplyEffectPostUnitStrategy
    {
        void Execute(UpgradeCard card, string matchId, string userId);
    }
}