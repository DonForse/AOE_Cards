using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private Button buttonToggleHandCards;
        [SerializeField] private GameObject unitCardsContainer;
        [SerializeField] private GameObject upgradeCardsContainer;
        [SerializeField] private Image btnToggleImage;
        [SerializeField] private Sprite imgUpgradesIcon;
        [SerializeField] private Sprite imgUnitIcon;
        private bool showingUpgrades;
        // Start is called before the first frame update
        void Start()
        {
            buttonToggleHandCards.onClick.AddListener(ToggleHandCards);
        }

        public void SetUnitCard(GameObject card)
        {
            card.transform.SetParent(unitCardsContainer.transform);
            card.transform.position = (unitCardsContainer.transform.position);
            card.transform.localScale = Vector3.one;
            card.GetComponent<Draggable>().enabled = true;
        }
        public void SetUpgradeCard(GameObject card) {
            card.transform.SetParent(upgradeCardsContainer.transform);
            card.transform.position = (upgradeCardsContainer.transform.position);
            card.transform.localScale = Vector3.one;
            card.GetComponent<Draggable>().enabled = true;
        }

        private void ToggleHandCards()
        {
            if (showingUpgrades)
                ShowHandUnits();
            else
                ShowHandUpgrades();

            ViewsHelper.RefreshView(GetComponent<RectTransform>());
        }

        public void ShowHandUnits()
        {
            showingUpgrades = false;
            //TODO: change image
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(false);
            unitCardsContainer.SetActive(true);

            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = true;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = false;

            btnToggleImage.sprite = imgUpgradesIcon;
            ViewsHelper.RefreshView(GetComponent<RectTransform>());
        }

        public void ShowHandUpgrades()
        {
            showingUpgrades = true;
            //TODO: change image
            //animations (control toggle from animation?)
            upgradeCardsContainer.SetActive(true);
            unitCardsContainer.SetActive(false);

            foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
                unitButton.interactable = false;

            foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
                upgradeButton.interactable = true;

            btnToggleImage.sprite = imgUnitIcon;
            ViewsHelper.RefreshView(GetComponent<RectTransform>());
        }

        internal IList<UnitCardView> GetUnitCards()
        {
            return unitCardsContainer.GetComponentsInChildren<UnitCardView>().ToList();
        }

        internal IList<UpgradeCardView> GetUpgradeCards()
        {
            return upgradeCardsContainer.GetComponentsInChildren<UpgradeCardView>().ToList();
        }

        internal void Clear()
        {
            foreach (var card in GetUnitCards())
            {
                Destroy(card.gameObject);
            }
            foreach (var card in GetUpgradeCards())
            {
                Destroy(card.gameObject);
            }
        }

        internal void PutCards(IList<CardView> cards)
        {
            foreach (var card in cards)
            {
                var container = card.CardType == CardType.Unit ? unitCardsContainer : upgradeCardsContainer;
                card.transform.SetParent(container.transform);
                card.transform.position = (container.transform.position);
                card.transform.localScale = Vector3.one;
                card.GetComponent<CardSelectable>().enabled = false;
                card.GetComponent<Draggable>().enabled = true;
            }
            ViewsHelper.RefreshView(unitCardsContainer.GetComponent<RectTransform>());
            ViewsHelper.RefreshView(upgradeCardsContainer.GetComponent<RectTransform>());
        }
    }
}
