using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Cards.Domain.Upgrades
{
    public interface IPreCalculusCardStrategy
    {
        bool IsValid(UpgradeCard card);
        void Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId);
    }
}