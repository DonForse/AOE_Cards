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
    private static readonly int Hover = Animator.StringToHash("hover");
    private static readonly int PoweringUp = Animator.StringToHash("PoweringUp");
    private int basePower;

    public void SetCard(UnitCardData card, Action<Draggable> onPlayCallback, RectTransform dropAreaPlay, bool draggable)
    {
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
                .WithDropArea(dropAreaPlay);
        }

        archetypeSection.SetCard(card.archetype);
    }

    public void IncreasePowerAnimation(UpgradesView upgrades, int newPower)
    {
        animator.SetTrigger(PoweringUp);
        StartCoroutine(IncreasePowerInTime(newPower, 2f));
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        //animator.SetBool(Hover, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //animator.SetBool(Hover, false);
    }
}
