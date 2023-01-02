﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class DeckMother
    {
        public static Deck Create(IList<UnitCard> unitCards = null, IList<UpgradeCard> upgradeCards = null)
        {
            return new Deck
            {
                UnitCards = new ConcurrentStack<UnitCard>(unitCards ??= new List<UnitCard>()),
                UpgradeCards = new ConcurrentStack<UpgradeCard>(upgradeCards ??= new List<UpgradeCard>())
            };
        }
        
        public static Deck CreateWithRandomCards(int unitCardsCount = 0,int upgradeCardsCount = 0)
        {
            var unitCards = new List<UnitCard>();
            for (var i = 0; i < unitCardsCount; i++) 
                unitCards.Add(UnitCardMother.Create($"unit-{i}"));

            var upgradeCards = new List<UpgradeCard>();
            for (var i = 0; i < upgradeCardsCount; i++) 
                upgradeCards.Add(UpgradeCardMother.Create($"upgrade-{i}"));
            
            return new Deck
            {
                UnitCards = new ConcurrentStack<UnitCard>(unitCards),
                UpgradeCards = new ConcurrentStack<UpgradeCard>(upgradeCards)
            };
        }
    }
}