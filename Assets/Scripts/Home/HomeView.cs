using UnityEngine;
using UnityEngine.UI;

namespace Home
{
    public class HomeView : MonoBehaviour, IHomeView
    {
        [SerializeField]
        private readonly Button _playButton;
        private HomePresenter _presenter;

        // Start is called before the first frame update
        void Start()
        {
            _presenter = new HomePresenter(ServicesProvider.Instance.GetMatchService());
            _playButton.onClick.AddListener(PlayMatch);
        }

        private void PlayMatch()
        {
            _presenter.StartSearchingMatch();
            //navigator.

        }

        public void OnMatchFound()
        {
            this.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}