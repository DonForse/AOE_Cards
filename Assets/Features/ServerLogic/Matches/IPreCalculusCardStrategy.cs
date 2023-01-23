using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches
{
    public interface IPreCalculusCardStrategy
    {
        bool IsValid(UpgradeCard card);
        void Execute(UpgradeCard card, UnitCard unitCardPlayed, UnitCard rivalUnitCard, ServerMatch serverMatch, Round round, string userId);
    }
}