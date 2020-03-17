using System.Collections.Generic;
using Game;
using Home;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Editor
{
    public class HomePresenterShould
    {
        private HomePresenter _presenter;
        private IHomeView _view;
        private IMatchService _matchService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            _view = Substitute.For<IHomeView>();
            GivenMatchService();
            _presenter = new HomePresenter(_view, _matchService);
         }

        [Test]
        public void CallMatchServiceWhenGameStart()
        {
            WhenStartNewMatch();
            ThenMatchServiceIsCalled();
        }

        private void WhenStartNewMatch()
        {
            _presenter.StartSearchingMatch();
        }

        private void ThenMatchServiceIsCalled()
        {
            _matchService.Received(1).StartMatch(string.Empty, null);
        }


        private void GivenMatchService()
        {
            _matchService = Substitute.For<IMatchService>();
        }

    }
}