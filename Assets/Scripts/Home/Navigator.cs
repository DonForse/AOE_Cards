using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Infrastructure.Services;
using UnityEngine;

namespace Home
{
    public class Navigator : MonoBehaviour
    {
        public HomeView homeView;
        public GameView gameView;
        public LoginView loginView;

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

        public void OpenGameView(MatchStatus ms)
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
    }
}