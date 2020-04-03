using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ICardView
{
    public string CardName { get; private set; }
    public Sprite Image => artwork;
    public string Effect  => effect.text;

    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private Sprite artwork;
    [SerializeField] private CardArchetypeView archetypeSection;
    [SerializeField] private Animator animator;
    private static readonly int Hover = Animator.StringToHash("hover");

    public void SetCard(UpgradeCardData card)
    {
        CardName = card.cardName;
        cardName.text = card.cardName;
        effect.text = card.effect;
        artwork = card.artwork;

        archetypeSection.SetCard(card.archetypes);
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
