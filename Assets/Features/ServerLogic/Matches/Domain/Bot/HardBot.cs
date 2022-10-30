using System;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;

namespace ServerLogic.Matches.Domain.Bot
{
    public class HardBot : Bot
    {

        internal override void PlayUnit(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerCards["BOT"].UnitCard != null)
                return;
            var card = PickUnitCard(match);
            match.PlayUnitCard("BOT", card);
        }

        private IUnitCard PickUnitCard(Match match)
        {
            var cards = match.Board.PlayersHands["BOT"].UnitsCards;

            int maxScore = 0;
            IUnitCard cardToPick = cards.FirstOrDefault();
            foreach (var card in cards)
            {
                var score = EvaluateUnitCard(card, match);
                if (score > maxScore)
                {
                    cardToPick = card;
                    maxScore = score;
                }
            }
            return cardToPick;
        }

        private int EvaluateUnitCard(UnitCard card, Match match)
        {
            int score = 0;
            score += EvaluateUnitArchetype(card, match);
            score += EvaluateUnitPower(card, match);
            return score;
        }

        private int EvaluateUnitPower(UnitCard card, Match match)
        {
            int score = 0;
            var round = match.Board.RoundsPlayed.Last();
            var RivalUpgrades = match.GetUpgradeCardsByPlayer(round, match.Users.First(u => u.Id != "BOT").Id);
            score += card.BasePower;

            var roundsCount = match.Board.RoundsPlayed.Count;

            if (card.BonusVs.Count > 0 && roundsCount > 2) {
                if (RivalUpgrades.Count(ru => ru.Archetypes.Any(ruA => card.BonusVs.Any(bvA => bvA == ruA))) > roundsCount / 2)
                    score += card.PowerEffect;
            }
            return score;
        }

        private int EvaluateUnitArchetype(UnitCard card, Match match)
        {
            int score = 0;
            var round = match.Board.RoundsPlayed.Last();
            var upgrades = match.GetUpgradeCardsByPlayer(round,"BOT");
            var RivalUpgrades = match.GetUpgradeCardsByPlayer(round, match.Users.First(u => u.Id != "BOT").Id);
            foreach (var upgrade in upgrades) {
                if (upgrade == null)
                    continue;
                if (card.Archetypes.Any(c=>upgrade.Archetypes.Any(a => a == c)))
                    score += upgrade.BasePower;
                if (card.BonusVs.Count > 0) 
                    score -= card.BasePower / 3 - (2* (RivalUpgrades.Count(c => c.Archetypes.Any(a => card.Archetypes.Any(arq => a == arq))) - match.Board.RoundsPlayed.Count));
            }
            return score;
        }

        internal override void PlayUpgrade(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerCards["BOT"].UpgradeCard != null)
                return;
            var card = PickUpgradeCard(match);
            match.PlayUpgradeCard("BOT", card);
        }

        private UpgradeCard PickUpgradeCard(Match match)
        {
            var cards = match.Board.PlayersHands["BOT"].UpgradeCards;

            int maxScore = 0;
            UpgradeCard cardToPick = cards.FirstOrDefault();
            foreach (var card in cards)
            {
                var score = EvaluateUpgradeCard(card, match);
                if (score > maxScore)
                {
                    cardToPick = card;
                    maxScore = score;
                }
            }
            return cardToPick;
        }

        private int EvaluateUpgradeCard(UpgradeCard card, Match match)
        {
            int score = 0;
            score += EvaluateUpgradeArchetype(card, match);
            score += EvaluateUpgradePower(card, match);
            return score;
        }

        private int EvaluateUpgradeArchetype(UpgradeCard card, Match match)
        {
            var units = match.Board.PlayersHands["BOT"].UnitsCards;
            var upgrades = match.Board.PlayersHands["BOT"].UpgradeCards;
            var playedUpgrades = match.GetUpgradeCardsByPlayer(match.Board.RoundsPlayed.Last(), "BOT");
            return units.Count(u => u.Archetypes.Any(ua => card.Archetypes.Any(ca => ca == ua))) 
                + (upgrades.Count(u => u.Archetypes.Any(ua => card.Archetypes.Any(ca => ca == ua))) *2)
                + (playedUpgrades.Count(pu => pu.Archetypes.Any(pua => card.Archetypes.Any(ca => ca == pua))) * 3);
        }

        private int EvaluateUpgradePower(UpgradeCard card, Match match)
        {
            var units = match.Board.PlayersHands["BOT"].UnitsCards;
            int score = 0;
            score += card.BasePower * units.Count(u => u.Archetypes.Any(ua => card.Archetypes.Any(c => c == ua)));
            if (card.BonusVs.Count > 0) {
                score -= (int)Math.Floor(card.BasePower - (units.Count(u => u.Archetypes.Any(ua => card.Archetypes.Any(c => c == ua))) * 1.3));
            }
            return score;
        }

        internal override void PlayReroll(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerReroll["BOT"])
                return;


            var units = match.Board.PlayersHands["BOT"].UnitsCards;
            var upgrades = match.Board.PlayersHands["BOT"].UpgradeCards;

            List<UnitCard> unitsToReroll = new List<UnitCard>();
            foreach (var card in units)
            {
                if (card.CardName.ToLower() == "villager")
                    continue;
                if (EvaluateUnitCard(card, match) < 15)
                {
                    unitsToReroll.Add(card);
                }
            }
            List<UpgradeCard> upgradesToReroll = new List<UpgradeCard>();
            foreach (var card in upgrades)
            {
                if (EvaluateUpgradeCard(card, match) < 15)
                {
                    upgradesToReroll.Add(card);
                }
            }
            RerollUnits(match, unitsToReroll);
            RerollUpgrades(match, upgradesToReroll);
            match.Board.RoundsPlayed.Last().PlayerReroll["BOT"] = true;
        }

        private static void RerollUnits(Match match, List<UnitCard> rerollCards)
        {
            var units = match.Board.PlayersHands["BOT"].UnitsCards;
            foreach (var card in rerollCards)
                units.Remove(card);

            for (int i = 0; i < rerollCards.Count; i++)
            {
                match.Board.PlayersHands["BOT"].UnitsCards.Add(match.Board.Deck.TakeUnitCards(1).First());
            }
            match.Board.Deck.AddUnitCards(rerollCards);
        }

        private static void RerollUpgrades(Match match, List<UpgradeCard> rerollCards)
        {
            var upgrades = match.Board.PlayersHands["BOT"].UpgradeCards;
            foreach (var card in rerollCards)
                upgrades.Remove(card);

            for (int i = 0; i < rerollCards.Count; i++)
            {
                match.Board.PlayersHands["BOT"].UpgradeCards.Add(match.Board.Deck.TakeUpgradeCards(1).First());
            }
            match.Board.Deck.AddUpgradeCards(rerollCards);
        }
    }
}

