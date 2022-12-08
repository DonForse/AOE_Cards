using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.Data;
using Features.Game.Scripts.Domain;
using TMPro;
using UnityEngine;

namespace Features.Game.Scripts.Delivery
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
        public override CardType CardType => CardType.Unit;

        public void SetCard(UnitCardData card)
        {
            if (card == null)
                return;
            this.name = card.cardName;
            CardName = card.cardName;
            cardName.text = card.cardName;
            effect.text = card.effect;
            basePower = card.power;
            power.text = card.power.ToString();
            artwork.sprite = card.artwork;
            artwork.preserveAspect = true;
            
            SetBackgroundColor(card.GetArchetypes());
            background.sprite = SetImageBackground(card.GetArchetypes());
            LoadAudioClip(card.GetArchetypes());
            //archetypeSection.SetCard(card.GetArchetypes());
        }

        private Sprite SetImageBackground(IList<Archetype> list)
        {
            foreach (var archetype in list)
            {
                switch (archetype) { 
                    case Archetype.Cavalry:
                        return Resources.Load<Sprite>("Cards/CardbackgroundsImages/steppe");
                    case Archetype.Archer:
                        return Resources.Load<Sprite>("Cards/CardbackgroundsImages/drygrass");
                    case Archetype.Militia:
                    case Archetype.Infantry:
                        return Resources.Load<Sprite>("Cards/CardbackgroundsImages/road");
                    default:
                        break;
                }
            }
            return Resources.Load<Sprite>("Cards/CardbackgroundsImages/land");
        }

        internal void LoadAudioClip(IList<Archetype> archetypes)
        {
            if (archetypes.Any(a => a == Archetype.Villager))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/villager");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/villager");
                return;
            }
            if (archetypes.Any(a => a == Archetype.Archer))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/archeryrange");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/unit");
                return;
            }
            if (archetypes.Any(a => a == Archetype.Cavalry))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/stable");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/cavalry");
                return;
            }
            if (archetypes.Any(a => a == Archetype.Infantry))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/barracks");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/unit");
                return;
            }
            if (archetypes.Any(a => a == Archetype.Siege))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/siege");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/catapult");
                return;
            }
            if (archetypes.Any(a => a == Archetype.Monk))
            {
                revealClip = Resources.Load<AudioClip>("Sounds/Units/monk");
                defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/unit");
                return;
            }
            defeatClip = Resources.Load<AudioClip>("Sounds/Units/dead/unit");
        }

        public void IncreasePowerAnimation(int newPower, float animationDuration)
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
                power.text = updatePower > 50 ? "∞" : (Mathf.CeilToInt(updatePower)).ToString();
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
