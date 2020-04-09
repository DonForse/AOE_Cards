using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Infrastructure.Services;
using Login;
using UnityEngine;

namespace Home
{
    public class Navigator : MonoBehaviour
    {
        public HomeView homeView;
        public GameView gameView;
        public LoginView loginView;
        public ResultView resultView;

        private IView currentActiveView;

        private void Start()
        {
            OpenLoginView();
        }

        public void OpenLoginView()
        {
            if (currentActiveView != null)
                currentActiveView.OnClosing();
            loginView.OnOpening();
            currentActiveView = loginView;
        }

        public void OpenGameView(Match ms)
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
    }
}