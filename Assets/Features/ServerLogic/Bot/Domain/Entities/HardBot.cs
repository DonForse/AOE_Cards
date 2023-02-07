﻿using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Bot.Domain.Entities
{
    public class HardBot : Bot
    {
        private readonly IPlayUpgradeCard _playUpgradeCard;
        private readonly IPlayUnitCard _playUnitCard;
        private readonly IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;

        public HardBot(IPlayUpgradeCard playUpgradeCard, IPlayUnitCard playUnitCard, IGetPlayerPlayedUpgradesInMatch getPlayerPlayedUpgradesInMatch)
            : base(playUpgradeCard, playUnitCard)
        {
            _getPlayerPlayedUpgradesInMatch = getPlayerPlayedUpgradesInMatch;
        }

        internal override void PlayUnit(ServerMatch serverMatch)
        {
            if (serverMatch.Board.CurrentRound.PlayerCards["BOT"].UnitCard != null)
                return;
            var card = PickUnitCard(serverMatch);
            _playUnitCard.Execute(serverMatch.Guid, "BOT", card.cardName);
        }

        private UnitCard PickUnitCard(ServerMatch serverMatch)
        {
            var cards = serverMatch.Board.PlayersHands["BOT"].UnitsCards;

            int maxScore = 0;
            UnitCard cardToPick = cards.FirstOrDefault();
            foreach (var card in cards)
            {
                var score = EvaluateUnitCard(card, serverMatch);
                if (score > maxScore)
                {
                    cardToPick = card;
                    maxScore = score;
                }
            }
            return cardToPick;
        }

        private int EvaluateUnitCard(UnitCard card, ServerMatch serverMatch)
        {
            int score = 0;
            score += EvaluateUnitArchetype(card, serverMatch);
            score += EvaluateUnitPower(card, serverMatch);
            return score;
        }

        private int EvaluateUnitPower(UnitCard card, ServerMatch serverMatch)
        {
            int score = 0;
            var rivalUpgrades = _getPlayerPlayedUpgradesInMatch.Execute(serverMatch.Guid,serverMatch.Users.First(u => u.Id != "BOT").Id);
            score += card.basePower;

            var roundsCount = serverMatch.Board.RoundsPlayed.Count;

            if (card.bonusVs.Any() && roundsCount > 2) {
                if (rivalUpgrades.Count(ru => ru.archetypes.Any(ruA => card.bonusVs.Any(bvA => bvA == ruA))) > roundsCount / 2)
                    score += card.powerEffect;
            }
            return score;
        }

        private int EvaluateUnitArchetype(UnitCard card, ServerMatch serverMatch)
        {
            int score = 0;
            var upgrades = _getPlayerPlayedUpgradesInMatch.Execute(serverMatch.Guid, "BOT");
            var rivalUpgrades = _getPlayerPlayedUpgradesInMatch.Execute(serverMatch.Guid,serverMatch.Users.First(u => u.Id != "BOT").Id);
            foreach (var upgrade in upgrades) {
                if (upgrade == null)
                    continue;
                if (card.archetypes.Any(c=>upgrade.archetypes.Any(a => a == c)))
                    score += upgrade.basePower;
                if (card.bonusVs.Any()) 
                    score -= card.basePower / 3 - (2* (rivalUpgrades.Count(c => c.archetypes.Any(a => card.archetypes.Any(arq => a == arq))) - serverMatch.Board.RoundsPlayed.Count));
            }
            return score;
        }

        internal override void PlayUpgrade(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.CurrentRound.PlayerCards["BOT"].UpgradeCard != null)
                return;
            var card = PickUpgradeCard(serverMatch);
            _playUpgradeCard.Execute(serverMatch.Guid, "BOT", card.cardName);
        }

        private UpgradeCard PickUpgradeCard(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            var cards = serverMatch.Board.PlayersHands["BOT"].UpgradeCards;

            int maxScore = 0;
            UpgradeCard cardToPick = cards.FirstOrDefault();
            foreach (var card in cards)
            {
                var score = EvaluateUpgradeCard(card, serverMatch);
                if (score > maxScore)
                {
                    cardToPick = card;
                    maxScore = score;
                }
            }
            return cardToPick;
        }

        private int EvaluateUpgradeCard(UpgradeCard card, Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            int score = 0;
            score += EvaluateUpgradeArchetype(card, serverMatch);
            score += EvaluateUpgradePower(card, serverMatch);
            return score;
        }

        private int EvaluateUpgradeArchetype(UpgradeCard card, ServerMatch serverMatch)
        {
            var units = serverMatch.Board.PlayersHands["BOT"].UnitsCards;
            var upgrades = serverMatch.Board.PlayersHands["BOT"].UpgradeCards;
            var playedUpgrades = _getPlayerPlayedUpgradesInMatch.Execute(serverMatch.Guid, "BOT");
            return units.Count(u => u.archetypes.Any(ua => card.archetypes.Any(ca => ca == ua))) 
                + (upgrades.Count(u => u.archetypes.Any(ua => card.archetypes.Any(ca => ca == ua))) *2)
                + (playedUpgrades.Count(pu => pu.archetypes.Any(pua => card.archetypes.Any(ca => ca == pua))) * 3);
        }

        private int EvaluateUpgradePower(UpgradeCard card, ServerMatch serverMatch)
        {
            var units = serverMatch.Board.PlayersHands["BOT"].UnitsCards;
            int score = 0;
            score += card.basePower * units.Count(u => u.archetypes.Any(ua => card.archetypes.Any(c => c == ua)));
            if (card.bonusVs.Any()) {
                score -= (int)Math.Floor(card.basePower - (units.Count(u => u.archetypes.Any(ua => card.archetypes.Any(c => c == ua))) * 1.3));
            }
            return score;
        }

        internal override void PlayReroll(ServerMatch serverMatch)
        {
            if (serverMatch.Board.CurrentRound.PlayerHasRerolled["BOT"])
                return;


            var units = serverMatch.Board.PlayersHands["BOT"].UnitsCards;
            var upgrades = serverMatch.Board.PlayersHands["BOT"].UpgradeCards;

            List<UnitCard> unitsToReroll = new List<UnitCard>();
            foreach (var card in units)
            {
                if (card.cardName.ToLower() == "villager")
                    continue;
                if (EvaluateUnitCard(card, serverMatch) < 15)
                {
                    unitsToReroll.Add(card);
                }
            }
            List<UpgradeCard> upgradesToReroll = new List<UpgradeCard>();
            foreach (var card in upgrades)
            {
                if (EvaluateUpgradeCard(card, serverMatch) < 15)
                {
                    upgradesToReroll.Add(card);
                }
            }
            RerollUnits(serverMatch, unitsToReroll);
            RerollUpgrades(serverMatch, upgradesToReroll);
            serverMatch.Board.CurrentRound.PlayerHasRerolled["BOT"] = true;
        }

        private static void RerollUnits(ServerMatch serverMatch, List<UnitCard> rerollCards)
        {
            var units = serverMatch.Board.PlayersHands["BOT"].UnitsCards;
            foreach (var card in rerollCards)
                units.Remove(card);

            for (int i = 0; i < rerollCards.Count; i++)
            {
                serverMatch.Board.PlayersHands["BOT"].UnitsCards.Add(serverMatch.Board.Deck.TakeUnitCards(1).First());
            }
            serverMatch.Board.Deck.AddUnitCards(rerollCards);
        }

        private static void RerollUpgrades(ServerMatch serverMatch, List<UpgradeCard> rerollCards)
        {
            var upgrades = serverMatch.Board.PlayersHands["BOT"].UpgradeCards;
            foreach (var card in rerollCards)
                upgrades.Remove(card);

            for (int i = 0; i < rerollCards.Count; i++)
            {
                serverMatch.Board.PlayersHands["BOT"].UpgradeCards.Add(serverMatch.Board.Deck.TakeUpgradeCards(1).First());
            }
            serverMatch.Board.Deck.AddUpgradeCards(rerollCards);
        }
    }
}
