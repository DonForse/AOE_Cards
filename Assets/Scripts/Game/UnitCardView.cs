using Game;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitCardView : MonoBehaviour, ICardView, IPointerEnterHandler, IPointerExitHandler
{
    public string CardName { get; private set; }

    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI effect;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private Image artwork;
    [SerializeField] private Image background;
    [SerializeField] private CardArchetypeView archetypeSection;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject cardback;

    private static readonly int Hover = Animator.StringToHash("hover");
    private static readonly int PoweringUp = Animator.StringToHash("PoweringUp");
    private int basePower;
    private bool isPlayable = false;

    public void SetCard(UnitCardData card, Action<Draggable> onPlayCallback, RectTransform dropAreaPlay, bool draggable, Action<bool> OnDrag)
    {
        this.name = card.cardName;
        CardName = card.cardName;
        cardName.text = card.cardName;
        effect.text = card.effect;
        basePower = card.power;
        power.text = card.power.ToString();
        artwork.sprite = card.artwork;
        artwork.preserveAspect = true;
        //background.sprite = card.background;


        if (draggable)
        {
            var draggableComponent = GetComponent<Draggable>();
            draggableComponent
                .WithCallback(onPlayCallback)
                .WithDragAction(OnDrag)
                .WithDropArea(dropAreaPlay);
        }

        archetypeSection.SetCard(card.archetype);
    }

    public void IncreasePowerAnimation(UpgradesView upgrades, int newPower, float animationDuration)
    {
        animator.SetTrigger(PoweringUp);
        StartCoroutine(IncreasePowerInTime(newPower, animationDuration));
    }

    IEnumerator IncreasePowerInTime(int newPower, float duration)
    {
        float n = 0;  // lerped value
        for (float f = 0; f <= duration; f += Time.deltaTime)
        {
            var updatePower = Mathf.Lerp(basePower, newPower, f / duration);
            power.text = ((int)updatePower).ToString();
            yield return null;
        }
    }

    public void ShowCardBack() {
        //animator.SetTrigger("backcard");
        cardback.SetActive(true);
    }

    public void ShowFrontCard()
    {
        //animator.SetTrigger("frontcard");
        cardback.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isPlayable)
            animator.SetTrigger("startglow");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetTrigger("stopglow");
    }
}
