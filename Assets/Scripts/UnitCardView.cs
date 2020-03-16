using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class UnitCardView : MonoBehaviour
{
    [SerializeField] private UnitCardData card;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private Sprite artwork;
    [SerializeField] private GameObject ArchetypeSection;

    public void Start()
    {
        SetCard();
    }

    private void SetCard()
    {
        cardName.text = card.cardName;
        effect.text = card.effect;
        power.text = card.power.ToString();
        artwork = card.artwork;

        foreach (var archetype in card.GetArchetypes())
        {
            //load resource
            //add prefab to archetypeSection
        }
    }

}
