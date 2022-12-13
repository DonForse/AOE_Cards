using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Users.Actions
{
    public class RemoveUserMatch : IRemoveUserMatch
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IUserMatchesRepository _userMatchesRepository;

        public RemoveUserMatch(IMatchesRepository matchesRepository, IUserMatchesRepository userMatchesRepository)
        {
            _matchesRepository = matchesRepository;
            _userMatchesRepository = userMatchesRepository;
        }

        public void Execute(string userId)
        {
            var matchId = _userMatchesRepository.GetMatchId(userId);
            
            _userMatchesRepository.Remove(userId);
            _matchesRepository.Remove(matchId);
        }
    }
}