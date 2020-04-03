using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICardView
{
    public string CardName { get; private set; }
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private Sprite artwork;
    [SerializeField] private CardArchetypeView archetypeSection;
    [SerializeField] private Animator animator;
    private static readonly int Hover = Animator.StringToHash("hover");

    public void SetCard(UnitCardData card)
    {
        CardName = card.cardName;
        cardName.text = card.cardName;
        effect.text = card.effect;
        power.text = card.power.ToString();
        artwork = card.artwork;

        archetypeSection.SetCard(card.archetype);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //animator.SetBool(Hover, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //animator.SetBool(Hover, false);
    }
}
