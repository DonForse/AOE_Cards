﻿using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches
{
    public class FurorCelticaApplyEffectPostUnitStrategy : IApplyEffectPostUnitStrategy
    {
        private readonly IMatchesRepository _matchesRepository;

        public FurorCelticaApplyEffectPostUnitStrategy(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
        }

        public void Execute(UpgradeCard upgradeCard, string matchId, string userId)
        {
            if (!IsValid(upgradeCard))
                return;
            
            var serverMatch = _matchesRepository.Get(matchId);
            var unitCardPlayed = serverMatch.Board.CurrentRound.PlayerCards[userId].UnitCard;
            
            if (!UnitCardIsSiege(unitCardPlayed)) return;
            
            serverMatch.Board.PlayersHands[userId].UnitsCards.Add(unitCardPlayed);
            _matchesRepository.Update(serverMatch);
        }

        private bool IsValid(UpgradeCard card) => card.cardName.ToLowerInvariant() == "furor celtica";

        private static bool UnitCardIsSiege(UnitCard unitCardPlayed) => unitCardPlayed.archetypes.Any(upArchetype => upArchetype == Archetype.SiegeUnit);
    }
}