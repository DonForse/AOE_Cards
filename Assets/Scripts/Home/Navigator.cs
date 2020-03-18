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

        private IView currentActiveView;

        private void Start()
        {
            OpenHomeView();
        }

        public void OpenGameView(MatchStatus ms)
        {
            currentActiveView.OnClosing();
            gameView.OnOpening();
            gameView.SetGame(ms);

            currentActiveView = gameView;
        }

        public void OpenHomeView()
        {            
            currentActiveView.OnClosing();
            homeView.OnOpening();
            currentActiveView = homeView;
        }
    }
}