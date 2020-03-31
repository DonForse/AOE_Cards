using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesView : MonoBehaviour
{
    [SerializeField] private GameObject playerUpgradesContainer;
    [SerializeField] private GameObject rivalUpgradesContainer;
    [SerializeField] private GameObject roundCardContainer;

    internal void SetRoundUpgradeCard(GameObject go)
    {
        go.transform.SetParent(roundCardContainer.transform);
    }
}
