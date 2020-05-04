using Infrastructure.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ShowdownView : MonoBehaviour
    {
        [SerializeField] private GameObject playerFieldContainer;
        [SerializeField] private GameObject rivalFieldContainer;
        [SerializeField] private TextMeshProUGUI _dropHereText;
        private UpgradeCardView _upgradeWait;
        private UnitCardView _unitWait;

        internal void PlayUpgradeCard(UpgradeCardView card, PlayerType playerType)
        {
            GameObject container;
            if (playerType == PlayerType.Player) {
                container = playerFieldContainer;
            }
            else
            {
                ClearUpgradeWaitCard();
                container = rivalFieldContainer;
                _upgradeWait = card;
                _upgradeWait.ShowCardBack();
            }

            card.transform.SetParent(container.transform);
            card.transform.position = (container.transform.position);
            card.transform.rotation = (container.transform.rotation);
            card.transform.localScale = Vector3.one;
            RefreshView(container);
        }

        private void ClearUpgradeWaitCard()
        {
            if (_upgradeWait != null)
                Destroy(_upgradeWait.gameObject);
            _upgradeWait = null;
        }

        internal void PlayUnitCard(UnitCardView card, PlayerType playerType)
        {
            GameObject container;
            if (playerType == PlayerType.Player)
            {
                container = playerFieldContainer;
            }
            else
            {
                ClearUnitWaitCard();
                container = rivalFieldContainer;
                _unitWait = card;
                _unitWait.ShowCardBack();
            }
            card.transform.SetParent(container.transform);
            card.transform.position = (container.transform.position);
            card.transform.rotation = (container.transform.rotation);
            card.transform.localScale = Vector3.one;

            RefreshView(container);
        }
        
        internal void ShowRivalWaitUnit(GameObject go)
        {
            if (_unitWait != null)
                return;
            var card = GameObject.Instantiate(go);
            var unit = card.GetComponent<UnitCardView>();
            unit.ShowCardBack();
            card.transform.SetParent(rivalFieldContainer.transform);
            card.transform.position = (rivalFieldContainer.transform.position);
            card.transform.localScale = Vector3.one;
            _unitWait = unit;
        }

        internal void ShowRivalWaitUpgrade(GameObject go)
        {
            if (_upgradeWait != null)
                return;
            var card = GameObject.Instantiate(go);
            var upgrade = card.GetComponent<UpgradeCardView>();
            upgrade.ShowCardBack();
            card.transform.SetParent(rivalFieldContainer.transform);
            card.transform.position = (rivalFieldContainer.transform.position);
            card.transform.localScale = Vector3.one;
            _upgradeWait = upgrade;
        }        

        private void ClearUnitWaitCard()
        {
            if (_unitWait != null)
                Destroy(_unitWait.gameObject);
            _unitWait = null;
        }

        private static void RefreshView(GameObject container)
        {
            LayoutRebuilder.MarkLayoutForRebuild(container.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }

        internal void MoveCards(UpgradesView upgradesView)
        {
            var upgrades = playerFieldContainer.GetComponentsInChildren<UpgradeCardView>();
            foreach (var upgrade in upgrades) {
                upgradesView.SetUpgrade(upgrade.gameObject, PlayerType.Player);
            }

            var rivalUpgrades = rivalFieldContainer.GetComponentsInChildren<UpgradeCardView>();
            foreach (var upgrade in rivalUpgrades)
            {
                upgradesView.SetUpgrade(upgrade.gameObject, PlayerType.Rival);
            }
            var units = playerFieldContainer.GetComponentsInChildren<UnitCardView>();
            foreach (var unit in units) {
                GameObject.Destroy(unit.gameObject);
            }
            var rivalUnits = rivalFieldContainer.GetComponentsInChildren<UnitCardView>();
            foreach (var unit in rivalUnits)
            {
                GameObject.Destroy(unit.gameObject);
            }
        }

        internal IEnumerable<UnitCardView> GetUnitsCardsPlayed()
        {
            var result = new List<UnitCardView>();
            result.AddRange(playerFieldContainer.GetComponentsInChildren<UnitCardView>());
            result.AddRange(rivalFieldContainer.GetComponentsInChildren<UnitCardView>());
            return result;
        }

        internal void Clear()
        {
            var units = playerFieldContainer.GetComponentsInChildren<UnitCardView>();
            foreach (var unit in units)
            {
                GameObject.Destroy(unit.gameObject);
            }
            var rivalUnits = rivalFieldContainer.GetComponentsInChildren<UnitCardView>();
            foreach (var unit in rivalUnits)
            {
                GameObject.Destroy(unit.gameObject);
            }
        }

        internal void CardDrag(bool dragging)
        {
            if (dragging)
                _dropHereText.gameObject.SetActive(true);
            if (!dragging)
                _dropHereText.gameObject.SetActive(false);
        }

        internal IEnumerator RevealCards(Action onFinish)
        {
            if (_upgradeWait != null) { 
                _upgradeWait.ShowFrontCard();
                _upgradeWait = null;
            }
            if (_unitWait != null)
            {
                _unitWait.ShowFrontCard();
                _unitWait = null;
            }
            yield return new WaitForSeconds(1f);
            onFinish();
        }
    }
}
