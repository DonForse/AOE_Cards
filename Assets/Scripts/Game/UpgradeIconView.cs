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
        [SerializeField] private TextMeshProUGUI cardName;

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
            image.sprite = upgradeView.GetArchetypeImage();
            effect.text = upgradeView.Effect;
            cardName.text = upgradeView.CardName;
        }
    }
}