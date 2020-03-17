using UnityEngine;

namespace GamePlay
{
    public class GamePlayView : MonoBehaviour, IGameplayView
    {
        private GamePlayPresenter _presenter;
        // Start is called before the first frame update
        void Start()
        {
            _presenter = new GamePlayPresenter(this, ServicesProvider.Instance.GetMatchService(),new GetDeck(new InMemoryDeckProvider()) );
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