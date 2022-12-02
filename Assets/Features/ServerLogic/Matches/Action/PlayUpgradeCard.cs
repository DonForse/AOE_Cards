using System;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayUpgradeCard : IPlayUpgradeCard
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;
        public PlayUpgradeCard(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }
        public void Execute(string matchId,string userId, string cardname)
        {
            var match = _matchesRepository.Get(matchId);
            //get type of card.
            var upgradeCard = _cardRepository.GetUpgradeCard(cardname);
            
            PlayCard(userId, upgradeCard, match);
            _matchesRepository.Update(match);
        }

        private void PlayCard(string userId, UpgradeCard upgradeCard, ServerMatch match)
        {
            var currentRound = match.Board.RoundsPlayed.Last();

            if (!currentRound.PlayerCards.ContainsKey(userId))
                throw new ApplicationException("Player is not in Match");
            if (currentRound.PlayerCards[userId].UpgradeCard != null)
                throw new ApplicationException("Upgrade card has already been played");

            if (currentRound.RoundState != RoundState.Upgrade)
                throw new ApplicationException("Upgrade card sent but not expecting it");

            upgradeCard.Play(match, userId);

            if (IsUpgradePhaseOver(currentRound, match))
            {
                currentRound.ChangeRoundState(RoundState.Unit);
            }
        }
        
        private bool IsUpgradePhaseOver(Round currentRound, ServerMatch sm) => 
            currentRound.PlayerCards.Count(c => c.Value.UpgradeCard != null) >= sm.Users.Count;
    }
}