using System;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Service;

namespace Features.ServerLogic.Matches.Action
{
    public class CreateRound : ICreateRound
    {
        private readonly IGetMatch _getMatch;
        private readonly Random _random;
        
        public CreateRound(IGetMatch getMatch)
        {
            _random = new Random();
            _getMatch = getMatch;
        }

        public void Execute(string matchId)
        {
            var serverMatch = _getMatch.Execute(matchId);
            serverMatch.Board.RoundsPlayed.Add(serverMatch.Board.CurrentRound);
            var round = new Round(serverMatch.Users.Select(u=>u.Id))
            {
                RoundUpgradeCard = serverMatch.Board.Deck.TakeUpgradeCards(1).FirstOrDefault(),
                PlayerWinner = null,
                roundNumber = serverMatch.Board.RoundsPlayed.Count + 1,
                NextBotActionTimeInSeconds = 
                    _random.Next( new ServerConfiguration().GetMaxBotWaitForPlayRoundTimeInSeconds(),
                        new ServerConfiguration().GetRoundTimerDurationInSeconds())
            };
            if (round.roundNumber == 3 || round.roundNumber == 6)
                round.ChangeRoundState(RoundState.Reroll);
            else
                round.ChangeRoundState(RoundState.Upgrade);

            serverMatch.Board.CurrentRound = round;
        }
    }
}