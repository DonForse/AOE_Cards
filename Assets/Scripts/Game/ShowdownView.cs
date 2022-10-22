using Common;
using Infrastructure.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Utilities;
using Infrastructure.Data;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ShowdownView : MonoBehaviour
    {
        [SerializeField] private GameObject playerFieldContainer;
        [SerializeField] private GameObject rivalFieldContainer;
        [SerializeField] private GameObject roundUpgradeContainer;
        [SerializeField] private TextMeshProUGUI _dropHereText;
        [SerializeField] private AudioClip battleHornClip;

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

            SetCard(card, container);

            ViewsHelper.RefreshView(container.GetComponent<RectTransform>());
            card.PlayRevealSound();
        }

        private static void SetCard(CardView card, GameObject container)
        {
            card.transform.SetParent(container.transform);
            card.transform.position = (container.transform.position);
            card.transform.rotation = (container.transform.rotation);
            card.transform.localScale = Vector3.one;
            card.GetComponent<Draggable>().enabled = false;
        }

        internal void SetRoundUpgradeCard(GameObject go)
        {
            go.transform.SetParent(roundUpgradeContainer.transform);
            go.transform.position = Vector3.zero;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.rotation = this.transform.rotation;
            ViewsHelper.RefreshView(roundUpgradeContainer.GetComponent<RectTransform>());
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
            card.GetComponent<Draggable>().enabled = false;

            ViewsHelper.RefreshView(container.GetComponent<RectTransform>());
            card.PlayRevealSound();
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
                    SetCard(upgrade, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? playerFieldContainer : rivalFieldContainer);
                    //PlayUpgradeCard(upgrade, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
                else
                {
                    if (lastRound.RivalReady)
                        ShowRivalWaitUpgrade();
                }

                if (cardPlayed.UnitCardData != null)
                {
                    var unit = Instantiator.Instance.CreateUnitCardGO(cardPlayed.UnitCardData);
                    SetCard(unit, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? playerFieldContainer : rivalFieldContainer);
                    //PlayUnitCard(unit, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
                else
                {
                    if (lastRound.RivalReady)
                        ShowRivalWaitUnit();
                }
            }
        }

        internal void Clear()
        {
            var player = playerFieldContainer.GetComponentsInChildren<CardView>();
            foreach (var card in player)
            {
                GameObject.Destroy(card.gameObject);
            }
            var rival = rivalFieldContainer.GetComponentsInChildren<CardView>();
            foreach (var card in rival)
            {
                GameObject.Destroy(card.gameObject);
            }
            var roundUpgrade = roundUpgradeContainer.GetComponentsInChildren<CardView>();
            foreach (var card in roundUpgrade)
            {
                GameObject.Destroy(card.gameObject);
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
                _upgradeWait.FlipCard(true, 1f);
                _upgradeWait.PlayRevealSound();
                _upgradeWait = null;
                yield return new WaitForSeconds(1f);

            }
            if (_unitWait != null)
            {
                _unitWait.FlipCard(true, 1f);
                _unitWait.PlayRevealSound();
                _unitWait = null;
                yield return new WaitForSeconds(1f);
                SoundManager.Instance.PlayAudioClip(battleHornClip, new AudioClipOptions { loop = false });
            }

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
