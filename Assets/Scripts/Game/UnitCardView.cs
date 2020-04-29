using System;
using System.Collections;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
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
        private static readonly int RevealCard = Animator.StringToHash("revealcard");
        private static readonly int Startglow = Animator.StringToHash("startglow");
        private static readonly int Stopglow = Animator.StringToHash("stopglow");

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
            StartCoroutine(FlipAnimation(1f));
            // cardback.SetActive(false);
        }
        private IEnumerator FlipAnimation(float duration)
        {
            float n = 0;  // lerped value
            for (float f = 0; f <= duration / 2; f += Time.deltaTime)
            {
                transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, 0, f / duration), transform.localScale.y, transform.localScale.z);
                yield return null;
            }
            cardback.SetActive(false);
            for (float f = 0; f <= duration / 2; f += Time.deltaTime)
            {
                transform.localScale = new Vector3(Mathf.Lerp(transform.localScale.x, 1, f / duration), transform.localScale.y, transform.localScale.z);
                yield return null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isPlayable)
                animator.SetTrigger(Startglow);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            animator.SetTrigger(Stopglow);
        }
    }
}
