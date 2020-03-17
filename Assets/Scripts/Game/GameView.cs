using System;
using UnityEngine;

namespace Game
{
    public class GameView : MonoBehaviour, IGameView
    {
        private GamePresenter _presenter;

        private void Awake()
        {
            _presenter = new GamePresenter(this, ServicesProvider.Instance.GetMatchService(),new GetDeck(new InMemoryDeckProvider()) );
        }

        // Start is called before the first frame update
        void Start()
        {
           _presenter.GameSetup();
        }
        public void ShowRoundEventCard(EventCardData eventCard)
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

        public void ShowEventCard()
        {
            throw new System.NotImplementedException();
        }

        public void ShowPlayerEventsPlayed()
        {
            throw new System.NotImplementedException();
        }

        public void ShowError()
        {
            throw new System.NotImplementedException();
        }

        public void EventCardSentPlay()
        {
            throw new System.NotImplementedException();
        }

        public void UnitCardSentPlay()
        {
            throw new System.NotImplementedException();
        }
    }
}