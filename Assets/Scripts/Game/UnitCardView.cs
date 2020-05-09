using System;
using System.Collections;
using Common;
using TMPro;
using UnityEngine;

namespace Game
{
    public class UnitCardView : CardView
    {
        [SerializeField] private TextMeshProUGUI effect;
        [SerializeField] private TextMeshProUGUI power;

        private static readonly int PoweringUp = Animator.StringToHash("PoweringUp");
        private int basePower;
        private bool isPlayable = false;
        private static readonly int RevealCard = Animator.StringToHash("revealcard");
        private static readonly int Startglow = Animator.StringToHash("startglow");
        private static readonly int Stopglow = Animator.StringToHash("stopglow");

        public void SetCard(UnitCardData card)
        {
            this.name = card.cardName;
            CardName = card.cardName;
            cardName.text = card.cardName;
            effect.text = card.effect;
            basePower = card.power;
            power.text = card.power.ToString();
            artwork.sprite = card.artwork;
            artwork.preserveAspect = true;
            
            SetBackgroundColor(card.GetArchetypes());
            archetypeSection.SetCard(card.GetArchetypes());
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
    }
}
