using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Game.Domain
{
    public interface IApplyEffectPostUnitStrategy
    {
        void Execute(UpgradeCard card, string matchId, string userId);
    }
}