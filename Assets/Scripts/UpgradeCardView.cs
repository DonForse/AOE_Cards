using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class UpgradeCardView : MonoBehaviour
{
    public string CardName { get; private set; }
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private Sprite artwork;
    [SerializeField] private GameObject archetypeSection;
    [SerializeField] private Animator animator;
    private static readonly int Hover = Animator.StringToHash("hover");

    public void SetCard(UpgradeCardData card)
    {
        CardName = card.cardName;
        cardName.text = card.cardName;
        effect.text = card.effect;
        artwork = card.artwork;

        //foreach (var archetype in card.GetArchetypes())
        //{
        //    //load resource
        //    //add prefab to archetypeSection
        //}
    }
    private void OnMouseOver()
    {
        animator.SetBool(Hover,true);
    }

    private void OnMouseExit()
    {
        animator.SetBool(Hover,false);
    }
}
