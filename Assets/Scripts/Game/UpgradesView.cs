using System;
using System.Collections;
using System.Linq;
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

        private ShowdownView _showdownView;

        public UpgradesView WithShowDownView(ShowdownView view)
        {
            _showdownView = view;
            return this;
        }

        internal IEnumerator SetRoundUpgradeCard(GameObject go, Action callback)
        {
            foreach (Transform child in roundCardContainer.transform)
            {
                child.gameObject.SetActive(false);
            }
            _showdownView.SetRoundUpgradeCard(go);
            yield return new WaitForSeconds(3f);

            SetRoundUpgrade(go);
            yield return new WaitForSeconds(2f);

            callback();
        }

        private void SetRoundUpgrade(GameObject go)
        {
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

        internal void SetGame(Match.Domain.Match match)
        {
            var rounds = match.Board.Rounds.Take(match.Board.Rounds.Count - 1);
            foreach (var round in rounds) {
                foreach (var cardPlayed in round.CardsPlayed) {
                    if (cardPlayed.UpgradeCardData == null)
                        continue;
                    var upgrade = Instantiator.Instance.CreateUpgradeCardGO(cardPlayed.UpgradeCardData);
                    SetUpgrade(upgrade.gameObject, PlayerPrefs.GetString(PlayerPrefsHelper.UserName) == cardPlayed.Player ? PlayerType.Player : PlayerType.Rival);
                }
            }
            var lastRound = match.Board.Rounds.Last();
            var upgradeRound = Instantiator.Instance.CreateUpgradeCardGO(lastRound.UpgradeCardRound);
            SetRoundUpgrade(upgradeRound.gameObject);
        }
    }
}
