using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowdownView : MonoBehaviour
{
    [SerializeField] private GameObject playerFieldContainer;
    [SerializeField] private GameObject rivalFieldContainer;
    [SerializeField] private TextMeshProUGUI _dropHereText;
    private GameObject _upgradeWait;
    private GameObject _unitWait;

    internal void PlayUpgradeCard(UpgradeCardView upgradeCardPlayed, PlayerType playerType)
    {
        //animation stuff
        ClearUpgradeWaitCard();
        var showDownContainer = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        upgradeCardPlayed.transform.SetParent(showDownContainer.transform);

        RefreshView(showDownContainer);
    }

    private void ClearUpgradeWaitCard()
    {
        if (_upgradeWait != null)
            Destroy(_upgradeWait);
        _upgradeWait = null;
    }

    internal void PlayUnitCard(UnitCardView unitCardPlayed, PlayerType playerType)
    {
        //animation stuff
        ClearUnitWaitCard();
        var container = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        unitCardPlayed.transform.SetParent(container.transform);

        RefreshView(container);
    }

    private void ClearUnitWaitCard()
    {
        if (_unitWait != null)
            Destroy(_unitWait);
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

    internal void ShowRivalWaitUnit(GameObject go)
    {
        if (_unitWait != null)
            return;
        var card = GameObject.Instantiate(go);
        var unit = card.GetComponent<UnitCardView>();
        unit.ShowCardBack();
        card.transform.SetParent(rivalFieldContainer.transform);
        card.transform.position = (rivalFieldContainer.transform.position);
        _unitWait = card;
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
        _upgradeWait = card;
    }
}
