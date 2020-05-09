using System;
using Infrastructure.Services;
using UnityEngine;

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
            iconView.SetUpgrade(upgradeView);
            go.SetActive(false);
            go.transform.SetParent(icon.transform);
        }
        
        internal void SetUpgrade(GameObject go, PlayerType playerType)
        {
            var container = playerType == PlayerType.Player ? playerUpgradesContainer : rivalUpgradesContainer;
            var icon = Instantiate(upgradeIconGo, container.transform);
            var iconView = icon.GetComponent<UpgradeIconView>();
            var upgradeView = go.GetComponent<UpgradeCardView>();
            iconView.SetUpgrade(upgradeView);
            go.SetActive(false);
            go.transform.SetParent(icon.transform);
        }

        internal void Clear()
        {
            foreach (var upgradeIcon in playerUpgradesContainer.GetComponentsInChildren<UpgradeIconView>())
                Destroy(upgradeIcon.gameObject);
            foreach (var upgradeIcon in rivalUpgradesContainer.GetComponentsInChildren<UpgradeIconView>())
                Destroy(upgradeIcon.gameObject);
            foreach (var upgradeIcon in roundCardContainer.GetComponentsInChildren<UpgradeIconView>())
                Destroy(upgradeIcon.gameObject);
        }

        internal void SetGame(Match match)
        {
            foreach (var round in match.Board.Rounds) {
                foreach (var cardPlayed in round.CardsPlayed) {
                    if (cardPlayed.UpgradeCardData == null)
                        continue;
                    //CreateCard(cardPlayed.UpgradeCardData);
              //      SetUpgrade(cardPlayed.UpgradeCardData, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
            }
            //SetRoundUpgradeCard();

        }
    }
}
