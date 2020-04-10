using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UpgradeCardView : MonoBehaviour, ICardView, IPointerEnterHandler, IPointerExitHandler
{
    public string CardName { get; private set; }
    public Sprite Image => artwork;
    public string Effect => effect.text;
    public int PowerEffect;

    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private Sprite artwork;
    [SerializeField] private CardArchetypeView archetypeSection;
    [SerializeField] private Animator animator;
    private static readonly int Hover = Animator.StringToHash("hover");

    private bool dragging = false;
    private bool _draggable = false;


    public void Update()
    {
        if (!_draggable || !dragging)
            return;

        transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public void SetCard(UpgradeCardData card, Action<Draggable> onPlayCallback, RectTransform dropAreaPlay, bool draggable)
    {
        CardName = card.cardName;
        cardName.text = card.cardName;
        effect.text = card.effect;
        artwork = card.artwork;
        PowerEffect = card.powerEffect;
        
        if (draggable)
        {
            var draggableComponent = GetComponent<Draggable>();
            draggableComponent
                .WithCallback(onPlayCallback)
                .WithDropArea(dropAreaPlay);
        }
        
        archetypeSection.SetCard(card.archetypes);
    }
    public Sprite GetArchetypeImage()
    {
        return archetypeSection.sprite;
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
