using Home;
using UnityEngine;
using UnityEngine.UI;

namespace Rules.Scripts.Delivery
{
    public class RulesView : MonoBehaviour, IView
    {
        [SerializeField] private Navigator _navigator;
        [SerializeField] private Button _backButton;

        public void OnOpening()
        {
            _backButton.onClick.AddListener(CloseView);
            this.gameObject.SetActive(true);
        }

        public void OnClosing()
        {
            _backButton.onClick.RemoveAllListeners();
            this.gameObject.SetActive(false);
        }

        private void CloseView()
        {
            _navigator.OpenHomeView();
        }
    }
}
