using System;
using System.Collections.Generic;
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
        public override CardType CardType => CardType.Upgrade;
        public IList<Archetype> Archetypes;

        [SerializeField] private TextMeshProUGUI effect;
      
        private bool dragging = false;
        private bool _draggable = false;

        public void Update()
        {
            if (!_draggable || !dragging)
                return;

            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        public void SetCard(UpgradeCardData card)
        {
            if (card == null)
                return;
            this.name = card.cardName;
            CardName = card.cardName;
            cardName.text = card.cardName;
            effect.text = card.effect;
            artwork.sprite = card.artwork;
            artwork.preserveAspect = true;
            PowerEffect = card.powerEffect;
            SetBackgroundColor(card.GetArchetypes());
            Archetypes = card.GetArchetypes();
            //archetypeSection.SetCard(card.GetArchetypes());
        }
        public Sprite GetArchetypeImage()
        {
            return archetypeSection.sprite;
        }
    }
}
