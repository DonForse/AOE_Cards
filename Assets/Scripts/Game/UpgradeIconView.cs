using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class UpgradeIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private GameObject hoverContainer;
        [SerializeField] private TextMeshProUGUI effect;

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverContainer.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverContainer.SetActive(false);
        }

        public void SetUpgrade(UpgradeCardView upgradeView)
        {
            image.sprite = upgradeView.Image;
            effect.text = upgradeView.Effect;
        }
    }
}