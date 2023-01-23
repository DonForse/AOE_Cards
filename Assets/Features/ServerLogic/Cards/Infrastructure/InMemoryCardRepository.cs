using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Matches.Domain;
using UnityEngine;

namespace Features.ServerLogic.Cards.Infrastructure
{
    public class InMemoryCardRepository : ICardRepository
    {
        private List<UnitCard> _unitCards;
        private List<UpgradeCard> _upgradeCards;

        public IList<UnitCard> GetUnitCards()
        {
            _unitCards ??= LoadUnitCards();
            return _unitCards.ToList();
        }

        public IList<UpgradeCard> GetUpgradeCards()
        {
            _upgradeCards ??= LoadUpgradeCards();
            return _upgradeCards.ToList();
        }

        public UnitCard GetUnitCard(string cardName)
        {
            cardName = cardName.ToLower();
            var card = _unitCards.FirstOrDefault(uc => uc.cardName.ToLower() == cardName);
            return card;
        }

        public UpgradeCard GetUpgradeCard(string cardName)
        {
            cardName = cardName.ToLower();
            var card = _upgradeCards.FirstOrDefault(uc => uc.cardName.ToLower() == cardName);
            return card;
        }

        private List<UpgradeCard> LoadUpgradeCards()
        {
            var cards = new List<UpgradeCard>();
            var ta = Resources.Load<TextAsset>("upgrades");

            foreach (var line in ta.text.Split('\n'))
            {
                var data = line.Split(',');
                UpgradeCard card = LoadUpgradeCard(data);
                cards.Add(card);
            }

            return cards;
        }

        private UpgradeCard LoadUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new UpgradeCard(data[3], int.Parse(data[4]),
                archetypesVs.Select(ConvertArchetype),
                archetypes.Select(ConvertArchetype));
            return card;
        }

        private List<UnitCard> LoadUnitCards()
        {
            var cards = new List<UnitCard>();
            var ta = Resources.Load<TextAsset>("units");
            var lines = ta.text.Split('\n');
            foreach (var line in lines) //'\n' en mac
            {
                var data = line.Split(',');
                var card = LoadUnitCard(data);
                cards.Add(card);
            }

            return cards;
        }

        private UnitCard LoadUnitCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesvs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new UnitCard(data[0].Trim('\n'), int.Parse(data[3]), int.Parse(data[6]),
                archetypesvs.Select(ConvertArchetype), archetypes.Select(ConvertArchetype));
            return card;
        }

        private Archetype ConvertArchetype(string c)
        {
            return c switch
            {
                "Archer" => Archetype.Archer,
                "Camel" => Archetype.Camel,
                "Cavalry" => Archetype.Cavalry,
                "Cavalry Archer" => Archetype.CavalryArcher,
                "Counter Unit" => Archetype.CounterUnit,
                "Eagle Warrior" => Archetype.EagleWarrior,
                "Eagle" => Archetype.EagleWarrior,
                "Elephant" => Archetype.Elephant,
                "Militia" => Archetype.Militia,
                "Monk" => Archetype.Monk,
                "Siege Unit" => Archetype.SiegeUnit,
                "Infantry" => Archetype.Infantry,
                "Villager" => Archetype.Villager,
                _ => Archetype.Villager
            };
        }
    }
}