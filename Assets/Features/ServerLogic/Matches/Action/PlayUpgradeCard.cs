using System;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;

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

            if (!PlayerExists(userId, currentRound))
                throw new ApplicationException("Player is not in Match");
            if (PlayerHaveAlreadyPlayedUpgradeCard(userId, currentRound))
                throw new ApplicationException("Upgrade card has already been played");
            if (!RoundIsInUpgrade(currentRound))
                throw new ApplicationException("Upgrade card sent but not expecting it");
            var hand = match.Board.PlayersHands[userId];
            if (IsUpgradeCardNull(upgradeCard))
                throw new ApplicationException("Invalid Upgrade card");
            if(!RemoveCardFromHand(upgradeCard, hand))
                throw new ApplicationException("Upgrade card is not in hand");
            
            currentRound.PlayerCards[userId].UpgradeCard = upgradeCard;

            if (!IsUpgradePhaseOver(currentRound, match))
                return;
            
            currentRound.ChangeRoundState(RoundState.Unit);
        }

        private static bool RemoveCardFromHand(UpgradeCard upgradeCard, Hand hand) => hand.UpgradeCards.Remove(upgradeCard);
        private static bool IsUpgradeCardNull(UpgradeCard upgradeCard) => upgradeCard == null;
        private static bool RoundIsInUpgrade(Round currentRound) => currentRound.RoundState == RoundState.Upgrade;
        private static bool PlayerHaveAlreadyPlayedUpgradeCard(string userId, Round currentRound) => currentRound.PlayerCards[userId].UpgradeCard != null;
        private static bool PlayerExists(string userId, Round currentRound) => currentRound.PlayerCards.ContainsKey(userId);
        private bool IsUpgradePhaseOver(Round currentRound, ServerMatch sm) => 
            currentRound.PlayerCards.Count(c => c.Value.UpgradeCard != null) >= sm.Users.Count;
    }
}