using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class GetMatch
    {
        private readonly IMatchesRepository _matchesRepository;

        public GetMatch(IMatchesRepository matchesRepository) {
            _matchesRepository = matchesRepository;
        }

        internal Domain.ServerMatch Execute(string matchId)
        {
            return _matchesRepository.Get(matchId);
        }
    }
}