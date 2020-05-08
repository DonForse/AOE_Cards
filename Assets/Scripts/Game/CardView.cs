using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public abstract class CardView : MonoBehaviour, ICardView, IPointerEnterHandler, IPointerExitHandler
    {
        public string CardName { get; internal set; }

        [SerializeField] internal TextMeshProUGUI cardName;
        [SerializeField] internal Image artwork;
        [SerializeField] internal Image background;
        [SerializeField] internal CardArchetypeView archetypeSection;
        [SerializeField] internal Animator animator;
        [SerializeField] internal GameObject cardback;
        [SerializeField] internal GameObject cardBackground;

        private readonly int Startglow = Animator.StringToHash("startglow");
        private readonly int Stopglow = Animator.StringToHash("stopglow");

        internal virtual void SetBackgroundColor(IList<Archetype> archetypes)
        {
            for (int i = 0; i < archetypes.Count; i++) {
                var go = new GameObject("background");
                go = Instantiate<GameObject>(go);
                var rt = go.AddComponent<RectTransform>();
                go.transform.SetParent(cardBackground.transform);

                float value = 1f / archetypes.Count;

                rt.anchorMin = new Vector2(value * i, 0);
                rt.anchorMax = new Vector2(value * (i+1),1);

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
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/gray");
                case Archetype.Siege:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/white");
                case Archetype.Monk:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/black");
                default:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/gray");
            }
        }

        public virtual void ShowCardBack() {
            cardback.SetActive(true);
        }

        public virtual void ShowFrontCard()
        {
            StartCoroutine(FlipAnimation(1f));
        }

        private IEnumerator FlipAnimation(float duration)
        {
            float n = 0; 
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

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            animator.SetTrigger(Startglow);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            animator.SetTrigger(Stopglow);
        }
    }
}
