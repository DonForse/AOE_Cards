using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Domain;
using UnityEngine;

namespace Features.ServerLogic.Cards.Infrastructure
{
    public class InMemoryCardRepository : ICardRepository
    {
        private List<UnitCard> UnitCards;
        private List<UpgradeCard> UpgradeCards;
        public IList<UnitCard> GetUnitCards()
        {
            if (UnitCards == null)
                UnitCards = LoadUnitCards();

            return UnitCards.ToList();
        }
        public IList<UpgradeCard> GetUpgradeCards()
        {
            if (UpgradeCards == null)
                UpgradeCards = LoadUpgradeCards();
            return UpgradeCards.ToList();
        }

        public UnitCard GetUnitCard(string cardName)
        {
            cardName = cardName.ToLower();
            var card = UnitCards.FirstOrDefault(uc => uc.CardName.ToLower() == cardName);
            return card;
        }

        public UpgradeCard GetUpgradeCard(string cardName)
        {
            cardName = cardName.ToLower();
            var card = UpgradeCards.FirstOrDefault(uc => uc.CardName.ToLower() == cardName);
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
            switch (data[3].ToLower())
            {
                case "furor celtica":
                    return SetFurorCelticaUpgradeCard(data);
                case "persian town center":
                    return SetPersianTCUpgradeCard(data);
                case "madrasah":
                    return SetMadrasahUpgradeCard(data);
                case "teutons faith":
                    return SetTeutonsFaithUpgradeCard(data);
                default:
                    return SetDefaultUpgradeCard(data);
            }
        }

        private UpgradeCard SetTeutonsFaithUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new TeutonsFaithUpgradeCard()
            {
                CardName = data[3],
                BasePower = int.Parse(data[4]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesVs.Select(c => ConvertArchetype(c)).ToList(),
            };
            return card;
        }

        private UpgradeCard SetMadrasahUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new MadrasahUpgradeCard()
            {
                CardName = data[3],
                BasePower = int.Parse(data[4]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesVs.Select(c => ConvertArchetype(c)).ToList(),
            };
            return card;
        }

        private UpgradeCard SetPersianTCUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new PersianTCUpgradeCard()
            {
                CardName = data[3],
                BasePower = int.Parse(data[4]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesVs.Select(c => ConvertArchetype(c)).ToList(),
            };
            return card;
        }

        private FurorCelticaUpgradeCard SetFurorCelticaUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new FurorCelticaUpgradeCard()
            {
                CardName = data[3],
                BasePower = int.Parse(data[4]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesVs.Select(c => ConvertArchetype(c)).ToList(),
            };
            return card;
        }

        private UpgradeCard SetDefaultUpgradeCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesVs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new UpgradeCard()
            {
                CardName = data[3],
                BasePower = int.Parse(data[4]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesVs.Select(c => ConvertArchetype(c)).ToList(),
            };
            return card;
        }

        private List<UnitCard> LoadUnitCards()
        {
            var cards = new List<UnitCard>();
            var ta = Resources.Load<TextAsset>("units");
            foreach (var line in ta.text.Split('\n')) //'\n' en mac
            {
                var data = line.Split(',');
                UnitCard card = LoadUnitCard(data);
                cards.Add(card); 
            }
            return cards;
        }

        private UnitCard LoadUnitCard(string[] data)
        {
            if (data[0].ToLower() == "villager")
                return SetVillagerUnitCard(data);

            return SetDefaultUnitCard(data);
        }

        private UnitCard SetVillagerUnitCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesvs = data[5].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var card = new UnitCard()
            {
                CardName = data[0],
                BasePower = int.Parse(data[3]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesvs.Select(c => ConvertArchetype(c)).ToList(),
                PowerEffect = int.Parse(data[6])
            };
            return card;
        }

        private UnitCard SetDefaultUnitCard(string[] data)
        {
            var archetypes = data[2].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var archetypesvs = data[5].Split(new char[] { '|'}, StringSplitOptions.RemoveEmptyEntries);
            var card = new UnitCard()
            {
                CardName = data[0].Trim('\n'),
                BasePower = int.Parse(data[3]),
                Archetypes = archetypes.Select(c => ConvertArchetype(c)).ToList(),
                BonusVs = archetypesvs.Select(c => ConvertArchetype(c)).ToList(),
                PowerEffect = int.Parse(data[6])
            };
            return card;
        }

        private Archetype ConvertArchetype(string c)
        {
            switch (c)
            {
                case "Archer":
                    return Archetype.Archer;
                case "Camel":
                    return Archetype.Camel;
                case "Cavalry":
                    return Archetype.Cavalry;
                case "Cavalry Archer":
                    return Archetype.CavalryArcher;
                case "Counter Unit":
                    return Archetype.CounterUnit;
                case "Eagle Warrior":
                case "Eagle":
                    return Archetype.EagleWarrior;
                case "Elephant":
                    return Archetype.Elephant;
                case "Militia":
                    return Archetype.Militia;
                case "Monk":
                    return Archetype.Monk;
                case "Siege Unit":
                    return Archetype.SiegeUnit;
                case "Infantry":
                    return Archetype.Infantry;
                case "Villager":
                    return Archetype.Villager;
                default:
                    return Archetype.Villager;
            }
        }

    }
}