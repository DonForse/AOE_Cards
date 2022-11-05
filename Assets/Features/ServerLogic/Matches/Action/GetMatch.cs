using ServerLogic.Matches.Infrastructure;

namespace ServerLogic.Matches.Action
{
    public class GetMatch
    {
        private readonly IMatchesRepository _matchesRepository;

        public GetMatch(IMatchesRepository matchesRepository) {
            _matchesRepository = matchesRepository;
        }

        internal Features.ServerLogic.Matches.Domain.ServerMatch Execute(string matchId)
        {
            return _matchesRepository.Get(matchId);
        }
    }
}