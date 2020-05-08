using System;
using Common;
using TMPro;
using UnityEngine;

namespace Game
{
    public class UpgradeCardView : CardView
    {
        public Sprite Image => artwork.sprite;
        public string Effect => effect.text;
        public int PowerEffect;

        [SerializeField] private TextMeshProUGUI effect;
      
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
            SetBackgroundColor(card.GetArchetypes());

            if (draggable)
            {
                var draggableComponent = GetComponent<Draggable>();
                draggableComponent
                    .WithCallback(onPlayCallback)
                    .WithDragAction(OnDrag)
                    .WithDropArea(dropAreaPlay);
            }

            archetypeSection.SetCard(card.GetArchetypes());
        }
        public Sprite GetArchetypeImage()
        {
            return archetypeSection.sprite;
        }
    }
}
