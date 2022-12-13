using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Users.Actions
{
    public class RemoveUserMatch : IRemoveUserMatch
    {
        private IMatchesRepository _matchesRepository;
        private IUserMatchesRepository _userMatchesRepository;

        public void Execute(string userId)
        {
            var matchId = _userMatchesRepository.GetMatchId(userId);
            
            _userMatchesRepository.Remove(userId);
            _matchesRepository.Remove(matchId);
        }
    }
}