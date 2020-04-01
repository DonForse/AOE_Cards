using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;

public class ShowdownView : MonoBehaviour
{
    [SerializeField] private GameObject playerFieldContainer;
    [SerializeField] private GameObject rivalFieldContainer;

    internal void PlayUpgradeCard(UpgradeCardView upgradeCardPlayed, PlayerType playerType)
    {
        //animation stuff
        var showDownContainer = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        upgradeCardPlayed.transform.SetParent(showDownContainer.transform);

        RefreshView(showDownContainer);
    }

    internal void PlayUnitCard(UnitCardView unitCardPlayed, PlayerType playerType)
    {
        //animation stuff
        var container = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        unitCardPlayed.transform.SetParent(container.transform);

        RefreshView(container);
    }

    private static void RefreshView(GameObject container)
    {
        LayoutRebuilder.MarkLayoutForRebuild(container.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    internal void Clear(UpgradesView upgradesView)
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
}
