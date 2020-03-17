using UnityEngine;
using UnityEngine.UI;

namespace Home
{
    public class HomeView : MonoBehaviour
    {
        [SerializeField]
        private readonly Button _playButton;
        private HomePresenter _presenter;

        // Start is called before the first frame update
        void Start()
        {
            _presenter = new HomePresenter();
            _playButton.onClick.AddListener(StartMatch);
        }

        private void StartMatch()
        {
            _presenter.StartMatch();
            //navigator.

        }

        void OnMatchFound()
        {
            this.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}