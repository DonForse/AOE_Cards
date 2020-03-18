using System;
using System.Text.RegularExpressions;
using Home;
using Infrastructure.Services;
using NSubstitute;
using NUnit.Framework;

namespace Editor
{
    public class HomePresenterShould
    {
        private HomePresenter _presenter;
        private IHomeView _view;
        private IMatchService _matchService;
        private readonly Action<MatchStatus> _action = status => { };

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

        [Test]
        public void InformViewWhenMatchServiceFoundGame()
        {
            WhenStartNewMatch();
            var ms = WhenMatchFound();
            ThenInformViewMatchFound(ms);
        }

        private MatchStatus WhenMatchFound()
        {
            //_presenter.When(x=>x.StartSearchingMatch()).Do(_ =>_action(new MatchStatus()));
            //fake pero algo asi
            return new MatchStatus();
        }

        private void ThenInformViewMatchFound(MatchStatus matchStatus)
        {
            _view.Received(1).OnMatchFound(matchStatus);
        }


        private void WhenStartNewMatch()
        {
            _presenter.StartSearchingMatch();
        }

        private void ThenMatchServiceIsCalled()
        {
            _matchService.Received(1).StartMatch(string.Empty, _action);
        }


        private void GivenMatchService()
        {
            _matchService = Substitute.For<IMatchService>();
        }
    }
}