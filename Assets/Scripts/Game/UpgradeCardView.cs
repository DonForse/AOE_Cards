﻿using System;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class UpgradeCardView : MonoBehaviour, ICardView, IPointerEnterHandler, IPointerExitHandler
    {
        public string CardName { get; private set; }
        public Sprite Image => artwork.sprite;
        public string Effect => effect.text;
        public int PowerEffect;

        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI effect;
        [SerializeField] private Image artwork;
        [SerializeField] private Image background;
        [SerializeField] private CardArchetypeView archetypeSection;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject cardback;
        private static readonly int Hover = Animator.StringToHash("hover");

        private bool dragging = false;
        private bool _draggable = false;


        public void Update()
        {
            if (!_draggable || !dragging)
                return;

            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        public void SetCard(UpgradeCardData card, Action<Draggable> onPlayCallback, RectTransform dropAreaPlay, bool draggable, Action<bool> OnDrag)
        {
            this.name = card.cardName;
            CardName = card.cardName;
            cardName.text = card.cardName;
            effect.text = card.effect;
            artwork.sprite = card.artwork;
            artwork.preserveAspect = true;
            PowerEffect = card.powerEffect;
        
            if (draggable)
            {
                var draggableComponent = GetComponent<Draggable>();
                draggableComponent
                    .WithCallback(onPlayCallback)
                    .WithDragAction(OnDrag)
                    .WithDropArea(dropAreaPlay);
            }
        
            archetypeSection.SetCard(card.archetypes);
        }
        public Sprite GetArchetypeImage()
        {
            return archetypeSection.sprite;
        }

        public void ShowCardBack()
        {
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
            animator.SetTrigger("startglow");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            animator.SetTrigger("stopglow");
        }
    }
}
