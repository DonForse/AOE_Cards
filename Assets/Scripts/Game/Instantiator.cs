using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    #region Singleton
    private static Instantiator _instance;

    public static Instantiator Instance { get { return _instance; } }

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
