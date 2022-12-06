using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace Features.ServerLogic.Matches.Action
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
        internal void Execute(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, RerollInfoDto cards)
        {
            var round = serverMatch.Board.RoundsPlayed.LastOrDefault();
            if (round.RoundState != RoundState.Reroll || round.PlayerReroll.ContainsKey(userId) && round.PlayerReroll[userId] )
                throw new ApplicationException("Reroll Not Available");

            if (IsRoundAlradyPlayed(round, userId))
                throw new ApplicationException("Reroll Not Available");

            var unitCards = new List<UnitCard>();
            GetUnitCards(serverMatch, userId, cards, unitCards);

            var upgradeCards = new List<UpgradeCard>();
            GetUpgradeCards(serverMatch, userId, cards, upgradeCards);

            bool revert = false;
            List<UpgradeCard> upgrades = GetNewHandUpgrades(serverMatch, userId, upgradeCards, ref revert);
            List<UnitCard> units = GetNewHandUnits(serverMatch, userId, unitCards, ref revert);

            if (revert)
                throw new ApplicationException("Invalid Reroll");

            round.PlayerReroll[userId] = true;
            AddNewUpgradeCards(serverMatch, userId, upgradeCards, upgrades);
            AddNewUnitCards(serverMatch, userId, unitCards, units);

            if (round.PlayerReroll.Values.Count(v=>v) == 2)
                serverMatch.Board.RoundsPlayed.LastOrDefault().ChangeRoundState(RoundState.Upgrade);

        }

        private static void AddNewUnitCards(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, List<UnitCard> unitCards, List<UnitCard> units)
        {
            serverMatch.Board.PlayersHands[userId].UnitsCards = units;
            for (int i = 0; i < unitCards.Count; i++)
            {
                serverMatch.Board.PlayersHands[userId].UnitsCards.Add(serverMatch.Board.Deck.TakeUnitCards(1).First());
            }
            serverMatch.Board.Deck.AddUnitCards(unitCards);
        }

        private static void AddNewUpgradeCards(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, List<UpgradeCard> upgradeCards, List<UpgradeCard> upgrades)
        {
            serverMatch.Board.PlayersHands[userId].UpgradeCards = upgrades;
            for (int i = 0; i < upgradeCards.Count; i++)
            {
                serverMatch.Board.PlayersHands[userId].UpgradeCards.Add(serverMatch.Board.Deck.TakeUpgradeCards(1).First());
            }
            serverMatch.Board.Deck.AddUpgradeCards(upgradeCards);
        }

        private List<UnitCard> GetNewHandUnits(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, List<UnitCard> unitCards, ref bool revert)
        {
            var units = serverMatch.Board.PlayersHands[userId].UnitsCards.ToList();
            foreach (var card in unitCards)
            {
                if (!units.Remove(card))
                    revert = true;
            }

            return units;
        }

        private List<UpgradeCard> GetNewHandUpgrades(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, List<UpgradeCard> upgradeCards, ref bool revert)
        {
            var upgrades = serverMatch.Board.PlayersHands[userId].UpgradeCards.ToList();
            foreach (var card in upgradeCards)
            {
                if (!upgrades.Remove(card))
                    revert = true;
            }
            return upgrades;
        }

        private void GetUpgradeCards(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, RerollInfoDto cards, List<UpgradeCard> upgradeCards)
        {
            foreach (var cardName in cards.upgradeCards)
            {
                var card = _getUpgradeCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!serverMatch.Board.PlayersHands[userId].UpgradeCards.Any(c => c.CardName == card.CardName))
                    throw new ApplicationException("Card Not Found");
                upgradeCards.Add(card);
            }
        }

        private void GetUnitCards(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId, RerollInfoDto cards, List<UnitCard> unitCards)
        {
            foreach (var cardName in cards.unitCards)
            {
                if (cardName.ToLower() == "villager")
                    continue;
                var card = _getUnitCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!serverMatch.Board.PlayersHands[userId].UnitsCards.Any(c => c.CardName == card.CardName))
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