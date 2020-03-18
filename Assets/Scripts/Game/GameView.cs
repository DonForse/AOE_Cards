using System;
using Home;
using Infrastructure.Services;
using UnityEngine;

namespace Game
{
    public class GameView : MonoBehaviour, IGameView, IView
    {
        private GamePresenter _presenter;

        private void Awake()
        {
            _presenter = new GamePresenter(this, ServicesProvider.Instance.GetMatchService());
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        public void SetGame(MatchStatus matchStatus)
        {
            _presenter.GameSetup(matchStatus);
        }

        public void ShowRoundUpgradeCard(UpgradeCardData upgradeCard)
        {
            throw new System.NotImplementedException();
        }

        public void ShowHand()
        {
            throw new System.NotImplementedException();
        }

        public void ShowUnitCard()
        {
            throw new System.NotImplementedException();
        }

        public void ShowRoundCard()
        {
            throw new System.NotImplementedException();
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

        public void OnOpening()
        {
            throw new NotImplementedException();
        }

        public void OnClosing()
        {
            throw new NotImplementedException();
        }
    }
}