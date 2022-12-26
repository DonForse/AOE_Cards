using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayUnitCard : IPlayUnitCard
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly ICardRepository _cardRepository;

        public PlayUnitCard(IMatchesRepository matchesRepository, ICardRepository cardRepository)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
        }

        public void Execute(string matchId,string userId, string cardname)
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
        
        public void PlayCard(string userId, UnitCard unitCard, ServerMatch match)
        {
            var currentRound = match.Board.RoundsPlayed.Last();

            if (!currentRound.PlayerCards.ContainsKey(userId))
                throw new ApplicationException("Player is not in Match");

            if (currentRound.PlayerCards[userId].UnitCard != null)//check swap
                throw new ApplicationException("Unit card has already been played");

            //if (currentRound.PlayerCards.Where(p=>p.Value.UpgradeCard != null).Count() < Users.Count)
            if (currentRound.RoundState != RoundState.Unit)
                throw new ApplicationException("Unit card sent but not expecting it");

            var upgrades = GetUpgradeCardsByPlayer(currentRound, userId, match);

            ApplicatePreUnitEffects(userId, upgrades, match);
            Play(match, userId, unitCard);
            ApplicatePostUnitEffects(userId, upgrades, match);

            if (IsRoundFinished(currentRound, match))
            {
                currentRound.ChangeRoundState(RoundState.Finished);
                DetermineWinner(match);
                if (!DetermineMatchWinner(match))
                    CreateNewRound(match);
            }
        }
        private bool IsRoundFinished(Round round, ServerMatch serverMatch) => round.PlayerCards.Count(pc => pc.Value.UnitCard != null) 
                                                                              == serverMatch.Users.Count;
        private List<UpgradeCard> GetUpgradeCardsByPlayer(Round currentRound, string userId, ServerMatch serverMatch)
        {
            var upgradeCards = serverMatch.Board.RoundsPlayed.SelectMany(r => r.PlayerCards
                .Where(pc => pc.Key == userId && pc.Value.UpgradeCard != null)
                .Select(pc => pc.Value.UpgradeCard)).ToList();
            upgradeCards.Add(currentRound.RoundUpgradeCard);
            return upgradeCards;
        }
        private void ApplicatePostUnitEffects(string userId, List<UpgradeCard> upgrades, ServerMatch serverMatch)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPostUnit(serverMatch, userId);
            }
        }

        private void ApplicatePreUnitEffects(string userId, List<UpgradeCard> upgrades, ServerMatch serverMatch)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPreUnit(serverMatch, userId);
            }
        }
        private  void Play(ServerMatch serverMatch, string userId, UnitCard card)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();

            var hand = serverMatch.Board.PlayersHands[userId];
            var unitCard = hand.UnitsCards.FirstOrDefault(u => u.CardName == card.CardName);

            if (unitCard == null)
                throw new ApplicationException("Unit card is not in hand");
            
            
            if (currentRound.PlayerCards.ContainsKey(userId) && currentRound.PlayerCards[userId].UnitCard != null)
                throw new ApplicationException("Unit card has already been played"); //TODO: Already Checked before
            
            if (card.CardName.ToLowerInvariant() == "villager") //TODO: Move to strategy
            {
                currentRound.PlayerCards[userId].UnitCard = card;
                return;
            }

            if(!hand.UnitsCards.Remove(unitCard)) //TODO: this is a validation for an already removed card
                throw new ApplicationException("Unit card is not in hand");

            currentRound.PlayerCards[userId].UnitCard = card;
        }
        
        private void DetermineWinner(ServerMatch serverMatch)
        {
            var currentRound = serverMatch.Board.RoundsPlayed.Last();

            //TODO: CAREFUL WITH TIES.
            var higherValueUser = new List<User>();
            var higgerValue = 0;
            foreach (var user in serverMatch.Users)
            {
                List<UpgradeCard> upgradeCards = GetUpgradeCardsByPlayer(currentRound, user.Id, serverMatch);
                List<UnitCard> vsUnits = GetVsUnits(currentRound, user.Id);
                var currentUnitCard = currentRound.PlayerCards[user.Id].UnitCard;
                ApplicatePreCalculusEffects(user.Id, upgradeCards, serverMatch);
                var power = currentUnitCard.CalculatePower(serverMatch, user.Id);
                ApplicatePostCalculusEffects(user.Id, upgradeCards, serverMatch);
                currentRound.PlayerCards[user.Id].UnitCardPower = power;
                if (power == higgerValue)
                {
                    higherValueUser.Add(user);
                }
                if (power > higgerValue)
                {
                    higherValueUser.Clear();
                    higgerValue = power;
                    higherValueUser.Add(user);
                }
            }
            currentRound.PlayerWinner = higherValueUser;
        }
        
        private List<UnitCard> GetVsUnits(Round currentRound, string userId)
        {
            return currentRound.PlayerCards
                .Where(pc => pc.Key != userId)
                .Select(c => c.Value.UnitCard).ToList();
        }

        private void ApplicatePreCalculusEffects(string userId, List<UpgradeCard> upgrades, ServerMatch serverMatch)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPreCalculus(serverMatch, userId);
            }
        }

        private void ApplicatePostCalculusEffects(string userId, List<UpgradeCard> upgrades, ServerMatch serverMatch)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPostCalculus(serverMatch, userId);
            }
        }
        
        private bool DetermineMatchWinner(ServerMatch serverMatch)
        {
            var winnersGrouped = serverMatch.Board.RoundsPlayed.SelectMany(r => r.PlayerWinner).GroupBy(c => c.Id);
            //TODO: Coul be a tie
            serverMatch.IsFinished = winnersGrouped.Any(w => w.Count() >= 4);
            if (!serverMatch.IsFinished)
                return false;

            serverMatch.IsTie = winnersGrouped.Count(w => w.Count() >= 4) > 1;

            serverMatch.MatchWinner = winnersGrouped.FirstOrDefault(w => w.Count() >= 4).First();
            if (serverMatch.IsFinished)
                serverMatch.Board.RoundsPlayed.Last().ChangeRoundState(RoundState.GameFinished);
            return true;

        }

        private void CreateNewRound(ServerMatch serverMatch)
        {
            var round = new Round(serverMatch.Users.Select(u=>u.Id))
            {
                RoundUpgradeCard = serverMatch.Board.Deck.TakeUpgradeCards(1).FirstOrDefault(),
                PlayerWinner = null,
                roundNumber = serverMatch.Board.RoundsPlayed.Count + 1,
                NextBotActionTimeInSeconds = new Random().Next( new ServerConfiguration().GetMaxBotWaitForPlayRoundTimeInSeconds(), new ServerConfiguration().GetRoundTimerDurationInSeconds())
            };
            if (round.roundNumber == 3 || round.roundNumber == 6)
                round.ChangeRoundState(RoundState.Reroll);
            else
                round.ChangeRoundState(RoundState.Upgrade);

            serverMatch.Board.RoundsPlayed.Add(round);
        }
    }
}