using System;
using Common;
using Match.Domain;
using NSubstitute;
using NUnit.Framework;
using Token;
using UniRx;
using Match = Match.Domain.Match;

namespace Home.Scripts.Tests.Editor
{
    public class HomePresenterShould
    {
        private HomePresenter _homePresenter;
        private ITokenService _tokenService;
        private IMatchService _matchService;
        private IHomeView _view;
        private ISubject<Unit> _playMatchSubject;
        private ISubject<Unit> _playVersusHardBotSubject;
        private ISubject<Unit> _playVersusEasyBotSubject;
        private IPlayerPrefs _playerPrefs;

        [SetUp]
        public void Setup()
        {
            _view = Substitute.For<IHomeView>();
            _playMatchSubject = new Subject<Unit>();
            _playVersusHardBotSubject = new Subject<Unit>();
            _playVersusEasyBotSubject = new Subject<Unit>();
            _view.OnPlayMatch().Returns(_playMatchSubject);
            _view.OnPlayVersusHardBot().Returns(_playVersusHardBotSubject);
            _view.OnPlayVersusEasyBot().Returns(_playVersusEasyBotSubject);

            _matchService = Substitute.For<IMatchService>();
            _tokenService = Substitute.For<ITokenService>();
            _playerPrefs = Substitute.For<IPlayerPrefs>();

            _homePresenter = new HomePresenter(_view, _matchService, _tokenService, _playerPrefs);
        }

        [Test]
        public void StartSearchingForMatchWhenPlayMatch()
        {
            GivenPresenterIsInitialized();
            WhenPlayMatch();
            ThenSaveEmptyIdAndStartMatch();

            void ThenSaveEmptyIdAndStartMatch()
            {
                Received.InOrder(() =>
                {
                    _playerPrefs.Received(1).SetString(PlayerPrefsHelper.MatchId, string.Empty);
                    _playerPrefs.Received(1).Save();
                    _matchService.Received(1).StartMatch(false, false, string.Empty, 0);
                });
            }
        }
        
        [Test]
        public void StartSearchingForMatchVersusHardBotWhenPlayVersusHardBot()
        {
            GivenPresenterIsInitialized();
            WhenPlayVersusHardBot();
            ThenSaveEmptyIdAndStartMatchVersusHardBot();

            void WhenPlayVersusHardBot() => _playVersusHardBotSubject.OnNext(Unit.Default);
            void ThenSaveEmptyIdAndStartMatchVersusHardBot()
            {
                Received.InOrder(() =>
                {
                    _playerPrefs.Received(1).SetString(PlayerPrefsHelper.MatchId, string.Empty);
                    _playerPrefs.Received(1).Save();
                    _matchService.Received(1).StartMatch(true, false, string.Empty, 1);
                });
            }
        }
        
        [Test]
        public void StartSearchingForMatchVersusEasyBotWhenPlayVersusEasyBot()
        {
            GivenPresenterIsInitialized();
            WhenPlayVersusEasyBot();
            ThenSaveEmptyIdAndPlayVersusEasyBot();

            void WhenPlayVersusEasyBot() => _playVersusEasyBotSubject.OnNext(Unit.Default);

            void ThenSaveEmptyIdAndPlayVersusEasyBot()
            {
                Received.InOrder(() =>
                {
                    _playerPrefs.Received(1).SetString(PlayerPrefsHelper.MatchId, string.Empty);
                    _playerPrefs.Received(1).Save();
                    _matchService.Received(1).StartMatch(true, false, string.Empty, 0);
                });
            }
        }

        [Test]
        public void CallViewWhenStartMatchReturnsSuccessfully()
        {
            var match = new global::Match.Domain.Match();
            GivenPresenterIsInitialized();
            GivenStartMatchReturns(match);
            WhenPlayMatch();
            _view.Received(1).OnMatchFound(match);
        }

        [Test]
        public void KeepGettingMatchIfMatchReturnsNull()
        {
            GivenPresenterIsInitialized();
            GivenStartMatchReturns(null);
            WhenPlayMatch();
            _view.DidNotReceive().OnMatchFound(Arg.Any<global::Match.Domain.Match>());
            _matchService.Received(1).GetMatch();
        }

        private void GivenStartMatchReturns(global::Match.Domain.Match match) =>
            _matchService.StartMatch(Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<int>())
                .Returns(Observable.Return(match));

        private void GivenPresenterIsInitialized() => _homePresenter.Initialize();
        private void WhenPlayMatch() => _playMatchSubject.OnNext(Unit.Default);
    }
}