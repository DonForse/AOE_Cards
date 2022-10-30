using Data;
using UnityEngine;

namespace Game
{
    public class CardInstantiator : MonoBehaviour
    {
        #region Singleton
        private static CardInstantiator _instance;

        public static CardInstantiator Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        #endregion

        [SerializeField] private GameObject unitCardGo;
        [SerializeField] private GameObject upgradeCardGo;

        public UnitCardView CreateUnitCardGO(UnitCardData data) {
            var go = Instantiate(unitCardGo);
            var unitCard = go.GetComponent<UnitCardView>();
            unitCard.SetCard(data);
            return unitCard;
        }

        public UpgradeCardView CreateUpgradeCardGO(UpgradeCardData data)
        {
            var go = Instantiate(upgradeCardGo);
            var upgradeCard = go.GetComponent<UpgradeCardView>();
            upgradeCard.SetCard(data);
            return upgradeCard;
        }
    }
}
