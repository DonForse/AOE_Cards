using System.Linq;
using Infrastructure.Services;
using UnityEngine;

namespace Game
{
    public class GamePresenter
    {
        private Match _match;
        private int currentRound;
        private readonly IGameView _view;
        private readonly IPlayService _playService;
        private readonly ITokenService _tokenService;

        public GamePresenter(IGameView view, IPlayService playService, ITokenService tokenService)
        {
            _view = view;
            _playService = playService;
            _tokenService = tokenService;
        }

        public Hand GetHand()
        {
            return _match.Hand;
        }

        public void GameSetup(Match match)
        {
            _match = match;
            currentRound = _match.Board.Rounds.Count() -1;
            PlayerPrefs.SetString(PlayerPrefsHelper.MatchId, match.Id);

            _view.InitializeGame(_match);
        }

        public void StartNewRound()
        {
            currentRound++;
            GetRound();
        }

        public void PlayUpgradeCard(string cardName)
        {
            var card = _match.Hand.TakeUpgradeCard(cardName);
            _playService.PlayUpgradeCard(card.cardName, OnUpgradeCardPostComplete, OnError);
        }

        public void PlayUnitCard(string cardName)
        {
            var card = _match.Hand.TakeUnitCard(cardName);
            _playService.PlayUnitCard(card.cardName, OnUnitCardPostComplete, OnError);
        }

        public void GetRound()
        {
            _playService.GetRound(currentRound, OnGetRoundComplete, OnError);
        }

        private void OnGetRoundComplete(Round round)
        {
            if (round.WinnerPlayers.Count > 0)
            {
                if (!_match.Board.Rounds.Any(r => r.RoundNumber == round.RoundNumber))
                    _match.Board.Rounds.Add(round);
            }
            _view.OnGetRoundInfo(round);
        }

        private void OnError(long responseCode, string message)
        {
            if (responseCode == 401)
            {
                _tokenService.RefreshToken(OnRefreshTokenComplete, OnRefreshTokenError);
                return;
            }
            _view.ShowError(message);
        }

        private void OnRefreshTokenError(string error)
        {
            GameManager.SessionExpired();
        }

        private void OnRefreshTokenComplete(UserResponseDto response)
        {
            PlayerPrefs.SetString(PlayerPrefsHelper.UserId, response.guid);
            PlayerPrefs.SetString(PlayerPrefsHelper.UserName, response.username);
            PlayerPrefs.SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            PlayerPrefs.SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
        }

        private void OnUnitCardPostComplete(Hand hand)
        {
            _match.Hand = hand; 
            _view.UnitCardSentPlay(hand);
        }

        private void OnUpgradeCardPostComplete(Hand hand)
        {
            _match.Hand = hand;
            _view.UpgradeCardSentPlay();
        }

        internal bool IsMatchOver()
        {
            return _match.Board.Rounds.SelectMany(r => r.WinnerPlayers).GroupBy(wp=>wp).Any(group=>group.Count() >= 4);
        }
    }
}