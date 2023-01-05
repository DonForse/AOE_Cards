﻿using System;
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
        private readonly ICalculateRoundResult _calculateRoundResult;

        public PlayUnitCard(IMatchesRepository matchesRepository, ICardRepository cardRepository,
            ICalculateRoundResult calculateRoundResult)
        {
            _matchesRepository = matchesRepository;
            _cardRepository = cardRepository;
            _calculateRoundResult = calculateRoundResult;
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

        private void PlayCard(string userId, UnitCard unitCard, ServerMatch match)
        {
            var currentRound = match.Board.CurrentRound;

            if (!currentRound.PlayerCards.ContainsKey(userId))
                throw new ApplicationException("Player is not in Match");

            if (currentRound.PlayerCards[userId].UnitCard != null)//check swap
                throw new ApplicationException("Unit card has already been played");

            //if (currentRound.PlayerCards.Where(p=>p.Value.UpgradeCard != null).Count() < Users.Count)
            if (currentRound.RoundState != RoundState.Unit)
                throw new ApplicationException("Unit card sent but not expecting it");

            var upgrades = match.GetUpgradeCardsByPlayer(userId);

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
            var currentRound = serverMatch.Board.CurrentRound;

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
            var currentRound = serverMatch.Board.CurrentRound;

            //TODO: CAREFUL WITH TIES.
            var higherValueUser = new List<User>();
            var higgerValue = 0;
            _calculateRoundResult.Execute(serverMatch.Guid);

            // foreach (var user in serverMatch.Users)
            // {
            //     List<UpgradeCard> upgradeCards = GetUpgradeCardsByPlayer(currentRound, user.Id, serverMatch);
            //     ApplicatePostCalculusEffects(user.Id, upgradeCards, serverMatch);
            // }
            // currentRound.PlayerWinner = higherValueUser;
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
                serverMatch.Board.CurrentRound.ChangeRoundState(RoundState.GameFinished);
            return true;

        }

        private void CreateNewRound(ServerMatch serverMatch)
        {
            serverMatch.Board.RoundsPlayed.Add(serverMatch.Board.CurrentRound);
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

            serverMatch.Board.CurrentRound = round;
        }
    }
}