using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayUnitCard : IPlayUnitCard
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;
        private readonly ICalculateRoundResult _calculateRoundResult;
        private readonly ICalculateMatchResult _calculateMatchResult;
        private readonly ICreateRound _createRound;
        private readonly IApplyEffectPostUnit _applyEffectPostUnit;

        public PlayUnitCard(IMatchesRepository matchesRepository,
            ICardRepository cardRepository,
            ICalculateRoundResult calculateRoundResult,
            ICalculateMatchResult calculateMatchResult,
            ICreateRound createRound,
            IApplyEffectPostUnit applyEffectPostUnit)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
            _calculateRoundResult = calculateRoundResult;
            _calculateMatchResult = calculateMatchResult;
            _createRound = createRound;
            _applyEffectPostUnit = applyEffectPostUnit;
        }

        public void Execute(string matchId, string userId, string cardname)
        {
            var match = _matchesRepository.Get(matchId);
            if (match == null)
                throw new ApplicationException("Match doesnt exists");
            //get type of card.
            var unitCard = _cardRepository.GetUnitCard(cardname);
            if (unitCard == null)
                throw new ApplicationException("Card doesnt exists");
            PlayCard(userId, unitCard, match);

            _matchesRepository.Update(match);
        }

        private void PlayCard(string userId, UnitCard unitCard, ServerMatch match)
        {
            var currentRound = match.Board.CurrentRound;

            if (!currentRound.PlayerCards.ContainsKey(userId))
                throw new ApplicationException("Player is not in Match");

            if (currentRound.PlayerCards[userId].UnitCard != null) //check swap
                throw new ApplicationException("Unit card has already been played");

            //if (currentRound.PlayerCards.Where(p=>p.Value.UpgradeCard != null).Count() < Users.Count)
            if (currentRound.RoundState != RoundState.Unit)
                throw new ApplicationException("Unit card sent but not expecting it");

            var upgrades = match.GetUpgradeCardsByPlayer(userId);

            // _applyEffectPreUnit.Execute();
            Play(match, userId, unitCard);
            _applyEffectPostUnit.Execute(match.Guid, userId);

            if (IsRoundFinished(currentRound, match))
            {
                currentRound.ChangeRoundState(RoundState.Finished);
                _calculateRoundResult.Execute(match.Guid);
                if (!_calculateMatchResult.Execute(match.Guid))
                    _createRound.Execute(match.Guid);
            }
        }

        private bool IsRoundFinished(Round round, ServerMatch serverMatch) =>
            round.PlayerCards.Count(pc => pc.Value.UnitCard != null)
            == serverMatch.Users.Count;
        

        private void Play(ServerMatch serverMatch, string userId, UnitCard card)
        {
            var currentRound = serverMatch.Board.CurrentRound;

            var hand = serverMatch.Board.PlayersHands[userId];
            var unitCard = hand.UnitsCards.FirstOrDefault(u => u.cardName == card.cardName);

            if (unitCard == null)
                throw new ApplicationException("Unit card is not in hand");

            if (currentRound.PlayerCards.ContainsKey(userId) && currentRound.PlayerCards[userId].UnitCard != null)
                throw new ApplicationException("Unit card has already been played"); //TODO: Already Checked before

            if (card.cardName.ToLowerInvariant() == "villager") //TODO: Move to strategy
            {
                currentRound.PlayerCards[userId].UnitCard = card;
                return;
            }

            if (!hand.UnitsCards.Remove(unitCard)) //TODO: this is a validation for an already removed card
                throw new ApplicationException("Unit card is not in hand");

            currentRound.PlayerCards[userId].UnitCard = card;
        }
    }
}