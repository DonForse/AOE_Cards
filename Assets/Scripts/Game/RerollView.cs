using Common;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RerollView : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GridLayoutGroup gridContainer;

    private List<CardView> selectedCards = new List<CardView>();
    private Action<List<string>, List<string>> _rerollAction;
    
    
    private void OnEnable()
    {
        continueButton.interactable = true;
        continueButton.onClick.AddListener(SendReroll);
        ViewsHelper.RefreshView(this.gridContainer.GetComponent<RectTransform>());
    }

    private void OnDisable()
    {
        continueButton.onClick.RemoveAllListeners();
        Clear();
    }

    public RerollView WithRerollAction(Action<List<string>, List<string>> reroll)
    {
        _rerollAction = reroll;
        return this;
    }

    public void Clear()
    {
        foreach (var card in selectedCards)
        {
            Destroy(card.gameObject);
        }
        selectedCards = new List<CardView>();
    }

    private void OnCardSelect(CardSelectable card)
    {
        var unit = card.GetComponent<CardView>();
        unit.SetSelected();
        selectedCards.Add(card.GetComponent<CardView>());
    }
    private void OnCardUnselect(CardSelectable card)
    {
        var unit = card.GetComponent<CardView>();
        unit.SetUnSelected();
        selectedCards.Remove(unit);
    }

    public void SendReroll()
    {
        continueButton.interactable = false;
        foreach (Transform card in gridContainer.transform)
        {
            card.GetComponent<CardSelectable>().enabled = false;
        }

        var units= selectedCards.Where(c => c.CardType == CardType.Unit);
        var upgrades= selectedCards.Where(c => c.CardType == CardType.Upgrade);

        _rerollAction?.Invoke(upgrades.Select(c => c.CardName).ToList(), units.Select(c => c.CardName).ToList());
    }

    internal IEnumerator SwapCards(IEnumerable<CardView> newCards)
    {
        gridContainer.enabled = false;
        foreach (var card in newCards)
        {
            card.transform.SetParent(gridContainer.transform);
            card.transform.localScale = Vector3.one;
            card.transform.position = Vector3.down * 1000f;
            card.transform.rotation = this.transform.rotation;
        }
        
        foreach (var card in selectedCards) {
            StartCoroutine(card.FlipCard(false));
        }
        yield return new WaitForSeconds(1f);
        gridContainer.enabled = false;
        foreach (var card in selectedCards)
        {
            StartCoroutine(card.MoveToPoint(Vector3.down*1000f));
        }
        yield return new WaitForSeconds(2f);
        gridContainer.enabled = false;
        foreach (var card in newCards)
        {
            StartCoroutine(card.MoveToPoint(Vector3.zero* 1000f));
        }

        gridContainer.enabled = true;
        yield return new WaitForSeconds(2f);
    }

    internal void PutCards(IEnumerable<CardView> cards)
    {
        foreach (var card in cards.OrderBy(c=>c.CardType))
        {
            card.transform.SetParent(gridContainer.transform);
            card.transform.localScale = Vector3.one;
            card.transform.position = gridContainer.transform.position;
            card.transform.rotation = this.transform.rotation;
            card.GetComponent<Draggable>().enabled = false;
            var selectable = card.GetComponent<CardSelectable>();

            selectable.WithSelectAction(selectComponent => { OnCardSelect(selectComponent); })
                .WithUnselectAction(selectComponent => { OnCardUnselect(selectComponent); });
            selectable.enabled = true;
        }
        gridContainer.constraintCount = cards.Count(c => c.CardType == CardType.Unit);
    }
}
