using System;
using Home;
using ICSharpCode.NRefactory.Visitors;
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
        private ITokenService _tokenService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            _view = Substitute.For<IHomeView>();
            GivenMatchService();
            _presenter = new HomePresenter(_view, _matchService, _tokenService);
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
            //will fail until I can call the callback.
            WhenStartNewMatch();
            var ms = WhenMatchFound();
            ThenInformViewMatchFound(ms);
        }

        private Match WhenMatchFound()
        {
            // _matchService.When(x=>x.StartMatch("",Arg.Any<Action<MatchStatus>>()))
            //     .Do();
            //fake pero algo asi
            return new Infrastructure.Services.Match();
        }

        private void WhenStartNewMatch()
        {
            _presenter.StartSearchingMatch(true,false,"");
        }

        private void ThenMatchServiceIsCalled()
        {
            _matchService.Received(1).StartMatch(true,false,"",0);
        }

        private void ThenInformViewMatchFound(Infrastructure.Services.Match matchStatus)
        {
            _view.Received(1).OnMatchFound(Arg.Any<Infrastructure.Services.Match>());
        }

        private void GivenMatchService()
        {
            _matchService = Substitute.For<IMatchService>();
        }

        private void GivenTokenService() {
            _tokenService = Substitute.For<ITokenService>();
        }
    }
}