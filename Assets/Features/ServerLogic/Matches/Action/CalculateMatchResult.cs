using System.Linq;
using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;

namespace Features.ServerLogic.Matches.Action
{
    public class CalculateMatchResult : ICalculateMatchResult
    {
        private readonly IMatchesRepository _matchesRepository;

        public CalculateMatchResult(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
        }

        public bool Execute(string matchId)
        {
            var serverMatch = _matchesRepository.Get(matchId);
            
            var winnersGrouped = serverMatch.Board.RoundsPlayed.Concat(new []{serverMatch.Board.CurrentRound})
                .SelectMany(r => r.PlayerWinner)
                .GroupBy(c => c.Id);

            var playersThatFinished = winnersGrouped.Count(w => w.Count() >= 4);
            
            serverMatch.IsFinished = playersThatFinished > 0;
            serverMatch.IsTie = playersThatFinished > 1;
            
            if (!serverMatch.IsFinished)
                return false;
            
            serverMatch.MatchWinner = winnersGrouped.FirstOrDefault(w => w.Count() >= 4).First();
            if (serverMatch.IsFinished)
                serverMatch.Board.CurrentRound.ChangeRoundState(RoundState.GameFinished);
            
            _matchesRepository.Update(serverMatch);
            return true;
        }
    }
}