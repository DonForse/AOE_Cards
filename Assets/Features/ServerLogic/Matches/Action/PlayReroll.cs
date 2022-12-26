using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayReroll : IPlayReroll
    {
        private readonly IGetUnitCard _getUnitCard;
        private readonly IGetUpgradeCard _getUpgradeCard;

        public PlayReroll(IGetUnitCard getUnitCard, IGetUpgradeCard getUpgradeCard)
        {
            _getUnitCard = getUnitCard;
            _getUpgradeCard = getUpgradeCard;
        }

        public void Execute(ServerMatch serverMatch, string userId, RerollInfoDto cards)
        {
            var round = serverMatch.Board.RoundsPlayed.LastOrDefault();
            if (round == null)
                throw new ApplicationException("Round does not exist");

            if (round.RoundState != RoundState.Reroll)
                throw new ApplicationException("Reroll Not Available");
            
            if (IsRoundAlreadyPlayed(round, userId))
                throw new ApplicationException("Reroll Not Available");
            if (!PlayerCanRerollThisRound(userId, round))
                throw new ApplicationException("Reroll Not Available");

            var unitCards = new List<UnitCard>();
            GetUnitCards(serverMatch, userId, cards, unitCards);

            var upgradeCards = new List<UpgradeCard>();
            GetUpgradeCards(serverMatch, userId, cards, upgradeCards);

            bool revert = false;
            List<UnitCard> unitCardsNotRerolled = GetUnitsToStayInHand(serverMatch, userId, unitCards, ref revert);
            List<UpgradeCard> upgradeCardsNotRerolled = GetUpgradesToStayInHand(serverMatch, userId, upgradeCards, ref revert);

            //TODO:
            //this is really hard scenario in which a validation already happened and then in another thread the card is no longer in the hand (use concurrent dic to fix)
            // the revert should not really be necessary.
            
            if (revert)
                throw new ApplicationException("Invalid Reroll");

            round.PlayerHasRerolled[userId] = true;
            AddNewUnitCards(serverMatch, userId, unitCards, unitCardsNotRerolled);
            AddNewUpgradeCards(serverMatch, userId, upgradeCards, upgradeCardsNotRerolled);

            if (HasAllPlayersRerolled(round, serverMatch))
                round.ChangeRoundState(RoundState.Upgrade);
            
            //TODO: IsUpdated but because it exists in memory, the repo is not called update.
        }

        private static bool PlayerCanRerollThisRound(string userId, Round round)
        {
            return round.PlayerHasRerolled.ContainsKey(userId) && !round.PlayerHasRerolled[userId];
        }

        private static bool HasAllPlayersRerolled(Round round, ServerMatch serverMatch)
        {
            return round.PlayerHasRerolled.Values.Count(v=>v) == serverMatch.Users.Count;
        }

        private static void AddNewUnitCards(ServerMatch serverMatch, string userId, List<UnitCard> rerolledUnitCards, List<UnitCard> unitCardsNotRerolled)
        {
            SetHandToBeOnlyPersistedCards();
            AddNewCardsToHand();
            AddRerolledCardsToDeck();

            void SetHandToBeOnlyPersistedCards() => serverMatch.Board.PlayersHands[userId].UnitsCards = unitCardsNotRerolled;
            void AddNewCardsToHand()
            {
                for (int i = 0; i < rerolledUnitCards.Count; i++)
                    serverMatch.Board.PlayersHands[userId].UnitsCards
                        .Add(serverMatch.Board.Deck.TakeUnitCards(1).First());
            }
            void AddRerolledCardsToDeck() => serverMatch.Board.Deck.AddUnitCards(rerolledUnitCards);
        }

        private static void AddNewUpgradeCards(ServerMatch serverMatch, string userId, List<UpgradeCard> rerolledUpgradeCards, List<UpgradeCard> upgradeCardsNotRerolled)
        {
            SetHandToBeOnlyPersistedCards();
            AddNewCardsToHand();
            AddRerolledCardsToDeck();

            void SetHandToBeOnlyPersistedCards() => serverMatch.Board.PlayersHands[userId].UpgradeCards = upgradeCardsNotRerolled;
            void AddNewCardsToHand()
            {
                for (int i = 0; i < rerolledUpgradeCards.Count; i++)
                    serverMatch.Board.PlayersHands[userId].UpgradeCards
                        .Add(serverMatch.Board.Deck.TakeUpgradeCards(1).First());
            }
            void AddRerolledCardsToDeck() => serverMatch.Board.Deck.AddUpgradeCards(rerolledUpgradeCards);
        }

        private List<UnitCard> GetUnitsToStayInHand(ServerMatch serverMatch, string userId, List<UnitCard> unitCards, ref bool revert)
        {
            var units = serverMatch.Board.PlayersHands[userId].UnitsCards.ToList();
            foreach (var card in unitCards)
            {
                if (!units.Remove(card))
                    revert = true;
            }

            return units;
        }

        private List<UpgradeCard> GetUpgradesToStayInHand(ServerMatch serverMatch, string userId, List<UpgradeCard> upgradeCards, ref bool revert)
        {
            var upgrades = serverMatch.Board.PlayersHands[userId].UpgradeCards.ToList();
            foreach (var card in upgradeCards)
            {
                if (!upgrades.Remove(card))
                    revert = true;
            }
            return upgrades;
        }

        private void GetUpgradeCards(ServerMatch serverMatch, string userId, RerollInfoDto cards, List<UpgradeCard> upgradeCards)
        {
            foreach (var cardName in cards.upgradeCards)
            {
                var card = _getUpgradeCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!UpgradeCardIsInPlayerHand(serverMatch, userId, card))
                    throw new ApplicationException("Card Not Found");
                upgradeCards.Add(card);
            }
        }

        private static bool UpgradeCardIsInPlayerHand(ServerMatch serverMatch, string userId, UpgradeCard card) => 
            serverMatch.Board.PlayersHands[userId].UpgradeCards.Any(c => c.CardName == card.CardName);

        private void GetUnitCards(ServerMatch serverMatch, string userId, RerollInfoDto cards, List<UnitCard> unitCards)
        {
            cards.unitCards = cards.unitCards.Where(cardName=>!IsVillagerCard(cardName)).ToList();
            foreach (var cardName in cards.unitCards)
            {
                var card = _getUnitCard.Execute(cardName);
                if (card == null)
                    throw new ApplicationException("Card Not Found");
                if (!UnitCardIsInPlayerHand(serverMatch, userId, card))
                    throw new ApplicationException("Card Not Found");

                unitCards.Add(card);
            }
        }

        private static bool UnitCardIsInPlayerHand(ServerMatch serverMatch, string userId, UnitCard card) => 
            serverMatch.Board.PlayersHands[userId].UnitsCards.Any(c => c.CardName == card.CardName);

        private static bool IsVillagerCard(string cardName) => cardName.ToLower() == "villager";

        private static bool IsRoundAlreadyPlayed(Round round, string userId)
        {
            return round.PlayerCards[userId].UpgradeCard != null;
        }
    }
}