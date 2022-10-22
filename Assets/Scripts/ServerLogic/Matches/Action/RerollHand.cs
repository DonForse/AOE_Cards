﻿using System;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Actions;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Infrastructure.DTO;

namespace ServerLogic.Matches.Action
{
    internal class RerollHand
    {
        private readonly ICardRepository _cardRepository;
        private readonly GetUnitCard _getUnitCard;
        private readonly GetUpgradeCard _getUpgradeCard;

        public RerollHand(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
            _getUnitCard = new GetUnitCard(_cardRepository);
            _getUpgradeCard = new GetUpgradeCard(_cardRepository);
        }
        internal void Execute(Domain.Match match, string userId, RerollInfoDto cards)
        {
            var round = match.Board.RoundsPlayed.LastOrDefault();
            if (round.RoundState != RoundState.Reroll || round.PlayerReroll.ContainsKey(userId) && round.PlayerReroll[userId] )
                throw new ApplicationException("Reroll Not Available");

            if (IsRoundAlradyPlayed(round, userId))
                throw new ApplicationException("Reroll Not Available");

            var unitCards = new List<UnitCard>();
            GetUnitCards(match, userId, cards, unitCards);

            var upgradeCards = new List<UpgradeCard>();
            GetUpgradeCards(match, userId, cards, upgradeCards);

            bool revert = false;
            List<UpgradeCard> upgrades = GetNewHandUpgrades(match, userId, upgradeCards, ref revert);
            List<UnitCard> units = GetNewHandUnits(match, userId, unitCards, ref revert);

            if (revert)
                throw new ApplicationException("Invalid Reroll");

            round.PlayerReroll[userId] = true;
            AddNewUpgradeCards(match, userId, upgradeCards, upgrades);
            AddNewUnitCards(match, userId, unitCards, units);

            if (round.PlayerReroll.Values.Count(v=>v) == 2)
                match.Board.RoundsPlayed.LastOrDefault().ChangeRoundState(RoundState.Upgrade);

        }

        private static void AddNewUnitCards(Domain.Match match, string userId, List<UnitCard> unitCards, List<UnitCard> units)
        {
            match.Board.PlayersHands[userId].UnitsCards = units;
            for (int i = 0; i < unitCards.Count; i++)
            {
                match.Board.PlayersHands[userId].UnitsCards.Add(match.Board.Deck.TakeUnitCards(1).First());
            }
            match.Board.Deck.AddUnitCards(unitCards);
        }

        private static void AddNewUpgradeCards(Domain.Match match, string userId, List<UpgradeCard> upgradeCards, List<UpgradeCard> upgrades)
        {
            match.Board.PlayersHands[userId].UpgradeCards = upgrades;
            for (int i = 0; i < upgradeCards.Count; i++)
            {
                match.Board.PlayersHands[userId].UpgradeCards.Add(match.Board.Deck.TakeUpgradeCards(1).First());
            }
            match.Board.Deck.AddUpgradeCards(upgradeCards);
        }

        private List<UnitCard> GetNewHandUnits(Domain.Match match, string userId, List<UnitCard> unitCards, ref bool revert)
        {
            var units = match.Board.PlayersHands[userId].UnitsCards.ToList();
            foreach (var card in unitCards)
            {
                if (!units.Remove(card))
                    revert = true;
            }

            return units;
        }

        private List<UpgradeCard> GetNewHandUpgrades(Domain.Match match, string userId, List<UpgradeCard> upgradeCards, ref bool revert)
        {
            var upgrades = match.Board.PlayersHands[userId].UpgradeCards.ToList();
            foreach (var card in upgradeCards)
            {
                if (!upgrades.Remove(card))
                    revert = true;
            }
            return upgrades;
        }

        private void GetUpgradeCards(Domain.Match match, string userId, RerollInfoDto cards, List<UpgradeCard> upgradeCards)
        {
            foreach (var cardName in cards.upgradeCards)
            {
                var card = _getUpgradeCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!match.Board.PlayersHands[userId].UpgradeCards.Any(c => c.CardName == card.CardName))
                    throw new ApplicationException("Card Not Found");
                upgradeCards.Add(card);
            }
        }

        private void GetUnitCards(Domain.Match match, string userId, RerollInfoDto cards, List<UnitCard> unitCards)
        {
            foreach (var cardName in cards.unitCards)
            {
                if (cardName.ToLower() == "villager")
                    continue;
                var card = _getUnitCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!match.Board.PlayersHands[userId].UnitsCards.Any(c => c.CardName == card.CardName))
                    throw new ApplicationException("Card Not Found");

                unitCards.Add(card);
            }
        }

        private static bool IsRoundAlradyPlayed(Round round, string userId)
        {
            return round.PlayerCards[userId].UpgradeCard != null;
        }
    }
}