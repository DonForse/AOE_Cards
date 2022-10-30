using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Data
{
    public abstract class CardData : ScriptableObject
    {
        public string cardName;
        public string archetypes;

        public IList<Archetype> GetArchetypes()
        {
            var archetypesData = archetypes.Split('|');
            if (archetypesData.Length == 0)
                return new List<Archetype>();
            var archetypesList = new List<Archetype>();
            foreach (var arc in archetypesData)
                switch (arc.ToLower())
                {
                    case "counterunit":
                        archetypesList.Add(Archetype.CounterUnit);
                        break;
                    case "infantry":
                        archetypesList.Add(Archetype.Infantry);
                        break;
                    case "militia":
                        archetypesList.Add(Archetype.Militia);
                        break;
                    case "archer":
                        archetypesList.Add(Archetype.Archer);
                        break;
                    case "cavalry":
                        archetypesList.Add(Archetype.Cavalry);
                        break;
                    case "camel":
                        archetypesList.Add(Archetype.Camel);
                        break;
                    case "elephant":
                        archetypesList.Add(Archetype.Elephant);
                        break;
                    case "cavalryarcher":
                        archetypesList.Add(Archetype.CavalryArcher);
                        break;
                    case "villager":
                        archetypesList.Add(Archetype.Villager);
                        break;
                    case "monk":
                        archetypesList.Add(Archetype.Monk);
                        break;
                    case "siege":
                        archetypesList.Add(Archetype.Siege);
                        break;
                    case "eagle":
                        archetypesList.Add(Archetype.Eagle);
                        break;
                    default:
                        break;
                }
            return archetypesList;
        }
    }
}
