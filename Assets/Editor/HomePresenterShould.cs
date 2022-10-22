using Home;
using Infrastructure.Services;
using Match.Domain;
using NSubstitute;
using NUnit.Framework;
using Token;

namespace Editor
{
    public class HomePresenterShould
    {
        private HomePresenter _presenter;
        private IMatchService _matchService;
        private ITokenService _tokenService;

        private const int CardsInHand = 5;

        [SetUp]
        public void Setup()
        {
            GivenMatchService();
            _presenter = new HomePresenter(_matchService, _tokenService);
        }

        [Test]
        public void CallMatchServiceWhenGameStart()
        {
            WhenStartNewMatch();
            ThenMatchServiceIsCalled();
        }

        private Match.Domain.Match WhenMatchFound()
        {
            // _matchService.When(x=>x.StartMatch("",Arg.Any<Action<MatchStatus>>()))
            //     .Do();
            //fake pero algo asi
            return new Match.Domain.Match();
        }

        private void WhenStartNewMatch()
        {
            _presenter.StartSearchingMatch(true,false,"");
        }

        private void ThenMatchServiceIsCalled()
        {
            _matchService.Received(1).StartMatch(true,false,"",0);
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