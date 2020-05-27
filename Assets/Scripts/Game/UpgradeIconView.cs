using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class UpgradeIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject hoverContainer;
        [SerializeField] private TextMeshProUGUI effect;
        [SerializeField] private TextMeshProUGUI cardName;
        [SerializeField] private TextMeshProUGUI power;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (this.transform.parent.name == "RoundCardContainer") //hotfix horrible por el container.
            {
                hoverContainer.SetActive(true);
                return;
            }
            var rect = hoverContainer.GetComponent<RectTransform>();
            var parentRect = this.GetComponent<RectTransform>();
            if (this.transform.parent.name == "PlayerUpgradesContainer") //hotfix horrible por el container.
            {
                if (parentRect.anchoredPosition.x - rect.rect.width / 2 < 0)
                    rect.anchoredPosition = new Vector2(10f + rect.rect.width / 2 + 10f, rect.anchoredPosition.y);
                hoverContainer.SetActive(true);
                return;
            }
            if (this.transform.parent.name == "RivalUpgradesContainer") //hotfix horrible por el container.
            {
                if (parentRect.anchoredPosition.x > 310f)
                    rect.anchoredPosition = new Vector2(-rect.rect.width / 2 - 10f, rect.anchoredPosition.y);
                hoverContainer.SetActive(true);
                return;
            }
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverContainer.SetActive(false);
        }

        public void SetUpgrade(UpgradeCardView upgradeView)
        {
            //image.sprite = upgradeView.GetArchetypeImage();
            effect.text = upgradeView.Effect;
            cardName.text = upgradeView.CardName;
            power.text = upgradeView.PowerEffect;
            SetBackgroundColor(upgradeView.Archetypes);
        }

        internal virtual void SetBackgroundColor(IList<Archetype> archetypes)
        {
            for (int i = 0; i < archetypes.Count; i++)
            {
                var go = new GameObject("background");
                //go = Instantiate<GameObject>(go);
                var rt = go.AddComponent<RectTransform>();
                go.transform.SetParent(background.transform);
                go.transform.position = this.transform.position;
                go.transform.localPosition = this.transform.localPosition;
                go.transform.localScale = this.transform.localScale;
                float value = 1f / archetypes.Count;

                rt.anchorMin = new Vector2(value * i, 0);
                rt.anchorMax = new Vector2(value * (i + 1), 1);

                rt.offsetMin = new Vector2(0, 0);
                rt.offsetMax = new Vector2(0, 0);
                var image = go.AddComponent<Image>();

                image.sprite = GetArchetypeColorBackground(archetypes[i]);
            }
        }
        private Sprite GetArchetypeColorBackground(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Villager:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/gray");
                case Archetype.Camel:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/yellow");
                case Archetype.Elephant:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/cyan");
                case Archetype.Cavalry:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/green");
                case Archetype.Archer:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/red");
                case Archetype.CavalryArcher:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/orange");
                case Archetype.Militia:
                case Archetype.Infantry:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/blue");
                case Archetype.Eagle:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/purple");
                case Archetype.CounterUnit:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/pink");
                case Archetype.Siege:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/white");
                case Archetype.Monk:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/black");
                default:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/gray");
            }
        }
    }
}