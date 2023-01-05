using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class CalculateMatchResult : ICalculateMatchResult
    {
        private IGetMatch _getMatch;
        public bool Execute(string matchId)
        {
            var serverMatch = _getMatch.Execute(matchId);
            var winnersGrouped = serverMatch.Board.RoundsPlayed.SelectMany(r => r.PlayerWinner).GroupBy(c => c.Id);
            //TODO: Coul be a tie
            serverMatch.IsFinished = winnersGrouped.Any(w => w.Count() >= 4);
            if (!serverMatch.IsFinished)
                return false;

            serverMatch.IsTie = winnersGrouped.Count(w => w.Count() >= 4) > 1;

            serverMatch.MatchWinner = winnersGrouped.FirstOrDefault(w => w.Count() >= 4).First();
            if (serverMatch.IsFinished)
                serverMatch.Board.CurrentRound.ChangeRoundState(RoundState.GameFinished);
            return true;
        }
    }
}