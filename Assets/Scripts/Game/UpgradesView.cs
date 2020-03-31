﻿using UnityEngine;

namespace Game
{
    public class UpgradesView : MonoBehaviour
    {
        [SerializeField] private GameObject upgradeIconGo;
        [SerializeField] private GameObject playerUpgradesContainer;
        [SerializeField] private GameObject rivalUpgradesContainer;
        [SerializeField] private GameObject roundCardContainer;

        internal void SetRoundUpgradeCard(GameObject go)
        {
            foreach (Transform child in roundCardContainer.transform)
            { 
                child.gameObject.SetActive(false);
            }
            var icon = Instantiate(upgradeIconGo, roundCardContainer.transform);
            var iconView = icon.GetComponent<UpgradeIconView>();
            var upgradeView = go.GetComponent<UpgradeCardView>();
            iconView.SetImage(upgradeView.Image);
            go.SetActive(false);
            go.transform.SetParent(icon.transform);
        }
        
        internal void SetUpgrade(GameObject go, PlayerType playerType)
        {
            var container = playerType == PlayerType.Player ? playerUpgradesContainer : rivalUpgradesContainer;
            var icon = Instantiate(upgradeIconGo, container.transform);
            var iconView = icon.GetComponent<UpgradeIconView>();
            var upgradeView = go.GetComponent<UpgradeCardView>();
            iconView.SetImage(upgradeView.Image);
            go.SetActive(false);
            go.transform.SetParent(icon.transform);
        }
    }
}