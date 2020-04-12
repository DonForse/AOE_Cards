﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HandView : MonoBehaviour
{
    [SerializeField] private Button buttonToggleHandCards;
    [SerializeField] private GameObject unitCardsContainer;
    [SerializeField] private GameObject upgradeCardsContainer;
    private bool showingUpgrades;
    // Start is called before the first frame update
    void Start()
    {
        buttonToggleHandCards.onClick.AddListener(ToggleHandCards);
    }

    public void SetUnitCard(GameObject card)
    {
        card.transform.SetParent(unitCardsContainer.transform);
    }
    public void SetUpgradeCard(GameObject card) {
        card.transform.SetParent(upgradeCardsContainer.transform);
    }

    private void ToggleHandCards()
    {
        if (showingUpgrades)
            ShowHandUnits();
        else
            ShowHandUpgrades();
    }

    public void ShowHandUnits()
    {
        showingUpgrades = false;
        //TODO: change image
        //animations (control toggle from animation?)
        upgradeCardsContainer.SetActive(false);
        unitCardsContainer.SetActive(true);

        foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
            unitButton.interactable = true;

        foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
            upgradeButton.interactable = false;
    }

    public void ShowHandUpgrades()
    {
        showingUpgrades = true;
        //TODO: change image
        //animations (control toggle from animation?)
        upgradeCardsContainer.SetActive(true);
        unitCardsContainer.SetActive(false);

        foreach (var unitButton in unitCardsContainer.GetComponentsInChildren<Button>())
            unitButton.interactable = false;

        foreach (var upgradeButton in upgradeCardsContainer.GetComponentsInChildren<Button>())
            upgradeButton.interactable = true;
    }

    internal IList<GameObject> GetUnitCards()
    {
        return unitCardsContainer.GetComponentsInChildren<UnitCardView>().Select(t => t.gameObject).ToList();
    }

    internal IList<GameObject> GetUpgradeCards()
    {
        return upgradeCardsContainer.GetComponentsInChildren<UpgradeCardView>().Select(t => t.gameObject).ToList();
    }

    internal void Clear()
    {
        foreach (var card in GetUnitCards()) {
            Destroy(card.gameObject);
        }
        foreach (var card in GetUpgradeCards())
        {
            Destroy(card.gameObject);
        }
    }
}
