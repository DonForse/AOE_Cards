using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowdownView : MonoBehaviour
{
    [SerializeField] private GameObject playerFieldContainer;
    [SerializeField] private GameObject rivalFieldContainer;

    internal void PlayUpgradeCard(UpgradeCardView upgradeCardPlayed, PlayerType playerType)
    {
        //animation stuff
        var showDownContainer = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        upgradeCardPlayed.transform.SetParent(showDownContainer.transform);
        Canvas.ForceUpdateCanvases();
    }

    internal void PlayUnitCard(UnitCardView unitCardPlayed, PlayerType playerType)
    {
        //animation stuff
        var showDownContainer = playerType == PlayerType.Player ? playerFieldContainer : rivalFieldContainer;
        unitCardPlayed.transform.SetParent(showDownContainer.transform);
        Canvas.ForceUpdateCanvases();
    }
}
