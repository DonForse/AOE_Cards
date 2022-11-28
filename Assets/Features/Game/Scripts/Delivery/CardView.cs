using System.Collections;
using System.Collections.Generic;
using Features.Game.Scripts.Domain;
using Sound;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Features.Game.Scripts.Delivery
{
    public abstract class CardView : MonoBehaviour, ICardView, IPointerEnterHandler, IPointerExitHandler
    {
        public abstract CardType CardType { get; }
        public string CardName { get; internal set; }

        [SerializeField] internal TextMeshProUGUI cardName;
        [SerializeField] internal Image artwork;
        [SerializeField] internal Image background;
        [SerializeField] internal CardArchetypeView archetypeSection;
        [SerializeField] internal Animator animator;
        [SerializeField] internal GameObject cardback;
        [SerializeField] internal GameObject cardBackground;
        internal AudioClip revealClip;
        internal AudioClip defeatClip;


        private readonly int Startglow = Animator.StringToHash("startglow");
        
        private readonly int Stopglow = Animator.StringToHash("stopglow");
        private readonly int Selected = Animator.StringToHash("selected");
        

        internal virtual void SetBackgroundColor(IList<Archetype> archetypes)
        {
            for (int i = 0; i < archetypes.Count; i++) {
                var go = new GameObject("background");
                //go = Instantiate<GameObject>(go);
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
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/pink");
                case Archetype.Siege:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/white");
                case Archetype.Monk:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/black");
                default:
                    return Resources.Load<Sprite>("Cards/Cardbackgrounds/gray");
            }
        }

        public virtual IEnumerator MoveToPoint(Vector3 newPosition, float duration) {
            yield return StartCoroutine(MoveToPointAnimation(newPosition, duration));
        }

        private IEnumerator MoveToPointAnimation(Vector3 newPosition, float duration)
        {
            float n = 0;
            for (float f = 0; f <= duration; f += Time.deltaTime)
            {
                var x = Mathf.Lerp(transform.localPosition.x, newPosition.x, f / duration);
                var y = Mathf.Lerp(transform.localPosition.y, newPosition.y, f / duration);
                var z = Mathf.Lerp(transform.localPosition.z, newPosition.z, f / duration);
                transform.localPosition = new Vector3(x,y,z);
                yield return null;
            }
        }

        public virtual void PlayRevealSound()
        {
            if (revealClip != null)
                SoundManager.Instance.PlayAudioClip(revealClip,new AudioClipOptions { loop = false});
        }

        public virtual void PlayDefeatSound()
        {
            if (defeatClip != null)
                SoundManager.Instance.PlayAudioClip(defeatClip, new AudioClipOptions { loop = false });
        }

        public virtual void ShowCardBack() {
            cardback.SetActive(true);
        }

        public virtual void FlipCard(bool show, float duration)
        {
            FlipAnimation(duration, show);
        }

        public virtual void SetSelected()
        {
            animator.SetBool(Selected, true);
        }

        public virtual void SetUnSelected()
        {
            animator.SetBool(Selected,false);
        }

        private void FlipAnimation(float duration, bool activate)
        {
            ObservableTween.Tween(transform.localScale.x, 0, duration/2, ObservableTween.EaseType.Linear, 
                onCompleteTween:()=> 
                {
                    ObservableTween.Tween(transform.localScale.x, 1, duration/2, ObservableTween.EaseType.Linear)
                    .Subscribe(x => transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z));
                })
                .DoOnCompleted(()=> { cardback.SetActive(!activate); })
                .Subscribe(x => transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z));
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
