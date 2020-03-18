using System.Threading.Tasks;
using Infrastructure.Services;
using UnityEngine;

namespace Home
{
    public class HomePresenter
    {
        private readonly IMatchService _matchService;
        private readonly IHomeView _view;

        public HomePresenter(IHomeView view, IMatchService matchService)
        {
            _view = view;
            _matchService = matchService;
        }

        public void StartSearchingMatch()
        {
           _matchService.StartMatch(PlayerPrefs.GetString("id"), OnMatchStatusComplete);
           _view.OnStartLookingForMatch();
        }

        private void OnMatchStatusComplete(MatchStatus matchStatus)
        {
            _view.OnMatchFound(matchStatus);
        }
    }
}