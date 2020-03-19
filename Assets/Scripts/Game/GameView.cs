using System;
using System.Collections.Generic;
using Home;
using Infrastructure.Services;
using UnityEngine;

namespace Game
{
    public class GameView : MonoBehaviour, IGameView, IView
    {
        [SerializeField] private ServicesProvider _servicesProvider;
        [SerializeField] private GameObject CardGO;
        private GamePresenter _presenter;

        public void OnOpening()
        {
            _presenter = new GamePresenter(this, _servicesProvider.GetMatchService());
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            this.gameObject.SetActive(false);
        }

        public void SetGame(MatchStatus matchStatus)
        {
            _presenter.GameSetup(matchStatus);
        }

        public void ShowRoundUpgradeCard(UpgradeCardData upgradeCard)
        {
            //animation stuff.  
        }

        public void InitializeHand(Hand hand)
        {
            //create instances of cards and inactive them.
        }

        public void ShowPlayerHand(Hand hand)
        {
            ShowPlayerUnitsCard(hand.GetUnitCards());
            ShowPlayerUpgradeCards(hand.GetUpgradeCards());
        }

        private void ShowPlayerUpgradeCards(IList<UpgradeCardData> list)
        {
            //set active and do animation
            //maybe add reroll chances
        }

        private void ShowPlayerUnitsCard(IList<UnitCardData> cards)
        {
            //set active and do animation
            //maybe add reroll chances
        }

        public void ShowUnitCard()
        {
            //animation of making the card bigger when hover
        }

        public void ShowRoundCard()
        {
            //animation of making the card bigger when hover
        }

        public void ShowUpgradeCardsPlayedByPlayer(string player)
        {
            throw new System.NotImplementedException();
        }

        public void ShowError()
        {
            throw new System.NotImplementedException();
        }

        public void UpgradeCardSentPlay()
        {
            throw new System.NotImplementedException();
        }

        public void UnitCardSentPlay()
        {
            throw new System.NotImplementedException();
        }

        public void CardReveal(RoundResult roundResult)
        {
            throw new NotImplementedException();
        }
    }
}