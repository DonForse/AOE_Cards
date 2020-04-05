using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardArchetypeView : MonoBehaviour
{
    [SerializeField] GameObject CardArchetypeIconGo;
    [SerializeField] string pathToIcons;
    public Sprite sprite;

    public void SetCard(string archetypesSerialized)
    {
        var archetypes = GetArchetypes(archetypesSerialized);
        SetArchetypes(archetypes);
    }

    private void SetArchetypes(IList<Archetype> archetypes)
    {
        foreach (var archetype in archetypes) {
            var go = Instantiate(CardArchetypeIconGo, this.transform);
            sprite = GetSprite(archetype);
            var image = go.GetComponentsInChildren<Image>().LastOrDefault();
            if (image != null)
                image.sprite = sprite;
        }
    }

    private Sprite GetSprite(Archetype archetype)
    {
       return Resources.Load<Sprite>(string.Format("{0}/{1}", pathToIcons, archetype.ToString()));
    }

    private IList<Archetype> GetArchetypes(string archetypesstring)
    {
        var archetypesData = archetypesstring.Split('|');
        var archetypes = new List<Archetype>();
        foreach (var arc in archetypesData)
            switch (arc.ToLower())
            {
                case "soldier":
                    archetypes.Add(Archetype.Soldier);
                    break;
                case "militia":
                    archetypes.Add(Archetype.Militia);
                    break;
                case "archer":
                    archetypes.Add(Archetype.Archer);
                    break;
                case "cavalry":
                    archetypes.Add(Archetype.Cavalry);
                    break;
                case "camel":
                    archetypes.Add(Archetype.Camel);
                    break;
                case "elephant":
                    archetypes.Add(Archetype.Elephant);
                    break;
                case "cavalryarcher":
                    archetypes.Add(Archetype.CavalryArcher);
                    break;
                case "villager":
                    archetypes.Add(Archetype.Villager);
                    break;
                case "monk":
                    archetypes.Add(Archetype.Monk);
                    break;
                case "siege":
                    archetypes.Add(Archetype.Siege);
                    break;
                case "eagle":
                    archetypes.Add(Archetype.Eagle);
                    break;
                default:
                    break;
            }
        return archetypes;
    }
}
