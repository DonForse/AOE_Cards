using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches
{
    public class PersianTcPreCalculusCardStrategy : IPreCalculusCardStrategy
    {
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;
        private readonly IMatchesRepository _matchesRepository;

        public PersianTcPreCalculusCardStrategy(IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch, IMatchesRepository matchesRepository)
        {
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
            _matchesRepository = matchesRepository;
        }

        public void Execute(UpgradeCard card, string matchId, string userId)
        {
            if (!IsValid(card))
                return;
            var serverMatch = _matchesRepository.Get(matchId);
            var round = serverMatch.Board.CurrentRound;
            var unitCardPlayed = round.PlayerCards[userId].UnitCard;
            var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(serverMatch.Guid, userId);
            if (!unitCardPlayed.archetypes.ContainsAnyArchetype(Archetype.Villager))
                return;
            var power = 0;
            foreach (var upgrade in upgrades) {
                if (upgrade.cardName == card.cardName)
                    continue;
                power += CalculateValue(upgrade,round.PlayerCards[userId].UnitCard, GetVsUnits(round, userId));
            }

            card.basePower = power + unitCardPlayed.basePower;
        }

        private bool IsValid(UpgradeCard card) => card.cardName.ToLowerInvariant() == "persian town center";

        private int CalculateValue(UpgradeCard upgrade, UnitCard unitCard, UnitCard vsCard)
        {
            if (!UpgradeHasArchetype()) 
                throw new ApplicationException("card does not contain any archetype");
            
            if (!IsUpgradeCardOfUnitArchetype()) //upgrade does not match unit. 
                return 0;
            
            if (UpgradeBonusVersusIsNull() || UpgradeHasEmptyBonusVersus())
                return upgrade.basePower; //then type match and will apply effect

            if (!IsUnitCardOfBonusArchetype()) //this works on the idea that bonus upgrades do not apply effects if not versus the correct unit.
                return 0;
            //then bonus vs = unit type.
            return upgrade.basePower;

            bool IsUpgradeCardOfUnitArchetype() => 
                unitCard.archetypes.ContainsAnyArchetype(upgrade.archetypes);
            bool UpgradeHasArchetype() => upgrade.archetypes != null;
            bool UpgradeHasEmptyBonusVersus() => !upgrade.bonusVs.Any();
            bool UpgradeBonusVersusIsNull() => upgrade.bonusVs == null;
            bool IsUnitCardOfBonusArchetype() => 
                upgrade.bonusVs.ContainsAnyArchetype(vsCard.archetypes);
        }
        
        private UnitCard GetVsUnits(Round currentRound, string userId)
        {
            return currentRound.PlayerCards
                .FirstOrDefault(pc => pc.Key != userId)
                .Value.UnitCard;
        }
    }
}