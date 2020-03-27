using Home;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Login
{
    public class LoginView : MonoBehaviour, IView, ILoginView
    {
        [SerializeField] private Button loginButton;
        [SerializeField] private Button registerButton;
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private Navigator navigator;
        private LoginPresenter _presenter;
        public void OnOpening()
        {
            _presenter = new LoginPresenter(this, servicesProvider.GetLoginService());
            loginButton.onClick.AddListener(Login);
            registerButton.onClick.AddListener(Register);
            this.gameObject.SetActive(true);
        }
        
        private void Login()
        {
            _presenter.Login(username.text, "a");
        }

        private void Register()
        {
            _presenter.Register(username.text, "a");
        }

        public void OnLoginComplete() {
            navigator.OpenHomeView();
        }
    
        public void OnLoginFail(string message)
        {
            errorMessage.text = message;
            errorMessage.gameObject.SetActive(true);

        }

        public void OnClosing()
        {
            loginButton.onClick.RemoveAllListeners();
            this.gameObject.SetActive(false);
        }
    }
}
