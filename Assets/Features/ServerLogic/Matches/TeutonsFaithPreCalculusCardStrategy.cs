using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Extensions;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches
{
    public class TeutonsFaithPreCalculusCardStrategy : IPreCalculusCardStrategy
    {
        private readonly IMatchesRepository _matchRepository;

        public TeutonsFaithPreCalculusCardStrategy(IMatchesRepository matchesRepository)
        {
            _matchRepository = matchesRepository;
        }

        public void Execute(UpgradeCard card, string matchId, string userId)
        {
            if (!IsValid(card)) return;

            var serverMatch = _matchRepository.Get(matchId);
            var unitCardPlayed = serverMatch.Board.CurrentRound.PlayerCards[userId].UnitCard;
            var rivalUnitCard = serverMatch.Board.CurrentRound.PlayerCards[serverMatch.GetRival(userId)].UnitCard;
            if (unitCardPlayed.archetypes.All(a => a != Archetype.Cavalry))
                return;

            if (!RivalPlayedMonk())
                return;
            
            card.basePower= 1000; // do not use because of overflow: int.MaxValue;

            _matchRepository.Update(serverMatch);
            bool RivalPlayedMonk() => rivalUnitCard.archetypes.ContainsAnyArchetype(new List<Archetype> {Archetype.Monk});
        }

        private bool IsValid(UpgradeCard card) => card.cardName.ToLowerInvariant() == "teutons faith";
    }
}