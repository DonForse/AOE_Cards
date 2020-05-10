﻿using Common;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RerollView : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GridLayoutGroup gridContainer;

    private List<CardView> selectedUnitCards;
    private List<CardView> selectedUpgradeCards;

    private void OnEnable()
    {
        continueButton.onClick.AddListener(SendReroll);
        ViewsHelper.RefreshView(this.gridContainer.GetComponent<RectTransform>());
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveAllListeners();
        Clear();
    }

    private GamePresenter _presenter;

    public RerollView WithGamePresenter(GamePresenter presenter) {
        _presenter = presenter;
        return this;
    }

    public void Clear() {
        foreach (Transform card in gridContainer.transform) {
            Destroy(card.gameObject);
        }
        selectedUnitCards = new List<CardView>();
        selectedUpgradeCards = new List<CardView>();
    }

    public void AddUpgradeCards(IList<UpgradeCardView> cards)
    {
        foreach (var card in cards)
        {
            card.transform.SetParent(gridContainer.transform);
            card.transform.localScale = Vector3.one;
            card.transform.position = gridContainer.transform.position;
            card.transform.rotation = this.transform.rotation;
            card.GetComponent<Draggable>().enabled = false;
            var selectable = card.GetComponent<CardSelectable>();

            selectable.WithSelectAction(selectComponent => { OnUpgradeCardSelect(selectComponent); })
                .WithUnselectAction(selectComponent => { OnUpgradeCardUnSelect(selectComponent); });
            selectable.enabled = true;
        }
    }

    public void AddUnitCards(IList<UnitCardView> cards)
    {
        foreach (var card in cards)
        {
            if (card.CardName.ToLower() == "villager")
                continue;
            card.transform.SetParent(gridContainer.transform);
            card.transform.localScale = Vector3.one;
            card.transform.position = gridContainer.transform.position;
            card.transform.rotation = this.transform.rotation;
            card.GetComponent<Draggable>().enabled = false;
            var selectable = card.GetComponent<CardSelectable>();

            selectable.WithSelectAction(selectComponent=> { OnUnitCardSelect(selectComponent); })
                .WithUnselectAction(selectComponent => { OnUnitCardUnSelect(selectComponent); });
            selectable.enabled = true;
        }
        gridContainer.constraintCount = gridContainer.transform.childCount;
    }


    private void OnUnitCardSelect(CardSelectable card)
    {
        var unit = card.GetComponent<UnitCardView>();
        unit.SetSelected();
        selectedUnitCards.Add(card.GetComponent<UnitCardView>());
    }
    private void OnUnitCardUnSelect(CardSelectable card)
    {
        var unit = card.GetComponent<UnitCardView>();
        unit.SetUnSelected();
        selectedUnitCards.Remove(unit);
    }

    private void OnUpgradeCardSelect(CardSelectable card)
    {
        var upgrade = card.GetComponent<UpgradeCardView>();
        upgrade.SetSelected();
        selectedUpgradeCards.Add(card.GetComponent<UpgradeCardView>());
    }

    private void OnUpgradeCardUnSelect(CardSelectable card)
    {
        var upgrade = card.GetComponent<UpgradeCardView>();
        upgrade.SetUnSelected();
        selectedUpgradeCards.Remove(card.GetComponent<UpgradeCardView>());
    }

    public void SendReroll()
    {
        _presenter.SendReroll(selectedUpgradeCards.Select(c=>c.CardName).ToList(), selectedUnitCards.Select(c=>c.CardName).ToList());
    }

    internal void SwapCards(Hand hand, Action onComplete)
    {
        //some cute little fucking effect
        onComplete();
    }
}
