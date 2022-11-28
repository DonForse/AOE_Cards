using Features.Game.Scripts.Delivery;
using Features.Game.Scripts.Domain;
using Features.Match.Domain;
using Infrastructure.Services;
using Login;
using Login.Scripts.Delivery;
using Result;
using Rules.Scripts.Delivery;
using UnityEngine;

namespace Home
{
    public class Navigator : MonoBehaviour
    {
        public HomeView homeView;
        public GameView gameView;
        public LoginView loginView;
        public ResultView resultView;
        public RulesView rulesView;

        private IView currentActiveView;

        private void Start()
        {
            //not the best practice, but dont want to implement a new class right now.
#if UNITY_EDITOR
  Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = false;
#endif
            OpenLoginView();
        }

        public void OpenLoginView()
        {
            if (currentActiveView != null)
                currentActiveView.OnClosing();
            loginView.OnOpening();
            currentActiveView = loginView;
        }

        public void OpenGameView(GameMatch ms)
        {
            if(currentActiveView != null)
                currentActiveView.OnClosing();
            gameView.OnOpening();
            gameView.SetGame(ms);

            currentActiveView = gameView;
        }

        public void OpenHomeView()
        {            
            if(currentActiveView != null)
                currentActiveView.OnClosing();
            homeView.OnOpening();
            currentActiveView = homeView;
        }

        public void OpenResultView(MatchResult win) {
            if (currentActiveView != null)
                currentActiveView.OnClosing();
            resultView.OnOpening();
            resultView.SetResult(win);

            currentActiveView = resultView;
        }

        public void OpenTutorialView()
        {
            if (currentActiveView != null)
                currentActiveView.OnClosing();
            rulesView.OnOpening();
            currentActiveView = rulesView;
        }
    }
}