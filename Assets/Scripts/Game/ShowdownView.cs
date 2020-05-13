using Infrastructure.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            if (playerType == PlayerType.Player)
            {
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
            ViewsHelper.RefreshView(container.GetComponent<RectTransform>());
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

            ViewsHelper.RefreshView(container.GetComponent<RectTransform>());
        }

        public void ShowRivalWaitUnit()
        {
            if (_unitWait != null)
                return;

            var card = Instantiator.Instance.CreateUnitCardGO(null);
            card.transform.SetParent(rivalFieldContainer.transform);
            card.transform.position = (rivalFieldContainer.transform.position);
            card.transform.localScale = Vector3.one;
            card.ShowCardBack();
            _unitWait = card;
        }

        public void ShowRivalWaitUpgrade()
        {
            if (_upgradeWait != null)
                return;
            var card = Instantiator.Instance.CreateUpgradeCardGO(null);
            card.transform.SetParent(rivalFieldContainer.transform);
            card.transform.position = (rivalFieldContainer.transform.position);
            card.transform.localScale = Vector3.one;
            card.ShowCardBack();
            _upgradeWait = card;
        }

        private void ClearUnitWaitCard()
        {
            if (_unitWait != null)
                Destroy(_unitWait.gameObject);
            _unitWait = null;
        }

        internal void MoveCards(UpgradesView upgradesView)
        {
            var upgrades = playerFieldContainer.GetComponentsInChildren<UpgradeCardView>();
            foreach (var upgrade in upgrades)
            {
                upgradesView.SetUpgrade(upgrade.gameObject, PlayerType.Player);
            }

            var rivalUpgrades = rivalFieldContainer.GetComponentsInChildren<UpgradeCardView>();
            foreach (var upgrade in rivalUpgrades)
            {
                upgradesView.SetUpgrade(upgrade.gameObject, PlayerType.Rival);
            }
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

        internal IEnumerable<UnitCardView> GetUnitsCardsPlayed()
        {
            var result = new List<UnitCardView>();
            result.AddRange(playerFieldContainer.GetComponentsInChildren<UnitCardView>());
            result.AddRange(rivalFieldContainer.GetComponentsInChildren<UnitCardView>());
            return result;
        }

        internal void SetRound(Round lastRound)
        {
            foreach (var cardPlayed in lastRound.CardsPlayed)
            {
                if (cardPlayed.UpgradeCardData != null)
                {
                    var upgrade = Instantiator.Instance.CreateUpgradeCardGO(cardPlayed.UpgradeCardData);
                    PlayUpgradeCard(upgrade, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
                else
                {
                    if (lastRound.RivalReady)
                        ShowRivalWaitUpgrade();
                }

                if (cardPlayed.UnitCardData != null)
                {
                    var unit = Instantiator.Instance.CreateUnitCardGO(cardPlayed.UnitCardData);
                    PlayUnitCard(unit, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
                else
                {  if (lastRound.RivalReady)
                    ShowRivalWaitUnit();
                }
            }
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
            if (_upgradeWait != null)
            {
                StartCoroutine(_upgradeWait.FlipCard(true,1f));
                _upgradeWait = null;
            }
            if (_unitWait != null)
            {
                StartCoroutine(_unitWait.FlipCard(true, 1f));
                _unitWait = null;
            }
            yield return new WaitForSeconds(1f);
            onFinish();
        }

        public IEnumerator UnitShowdown(Round round, Action callbackComplete)
        {
            yield return RevealCards(() =>
            {
                foreach (var cardView in GetUnitsCardsPlayed())
                {
                    var card = round.CardsPlayed.FirstOrDefault(c => c.UnitCardData.cardName == cardView.CardName);
                    if (card == null)
                        continue;
                    cardView.IncreasePowerAnimation(card.UnitCardPower, 1f);
                }
            });
            yield return new WaitForSeconds(2f);
            callbackComplete?.Invoke();
        }
    }
}
