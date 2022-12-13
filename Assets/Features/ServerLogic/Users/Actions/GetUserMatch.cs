using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Users.Actions
{
    public class GetUserMatch : IGetUserMatch
    {
        private readonly IMatchesRepository _matchesRepository;
        private IUserMatchesRepository _userMatchesRepository;

        public GetUserMatch(IMatchesRepository matchesRepository, IUserMatchesRepository userMatchesRepository)
        {
            _matchesRepository = matchesRepository;
            _userMatchesRepository = userMatchesRepository;
        }

        public ServerMatch Execute(string userId)
        {
            var matchId = _userMatchesRepository.GetMatchId(userId);
            return _matchesRepository.Get(matchId);
        }
    }
}