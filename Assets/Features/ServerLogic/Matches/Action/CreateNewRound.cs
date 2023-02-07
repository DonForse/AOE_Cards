using System;
using System.Linq;
using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;

namespace Features.ServerLogic.Matches.Action
{
    public class CreateNewRound : ICreateNewRound
    {
        private readonly IMatchesRepository _currentMatchRepository;
        private readonly Random _random;
        
        public CreateNewRound(IMatchesRepository matchesRepository)
        {
            _random = new Random();
            _currentMatchRepository = matchesRepository;
        }

        public void Execute(string matchId)
        {
            var serverMatch = _currentMatchRepository.Get(matchId);
            if (serverMatch.Board.CurrentRound != null) 
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
            if (round.roundNumber == 1 || round.roundNumber == 3 || round.roundNumber == 6)
                round.ChangeRoundState(RoundState.Reroll);
            else
                round.ChangeRoundState(RoundState.Upgrade);

            serverMatch.Board.CurrentRound = round;
            _currentMatchRepository.Update(serverMatch);
        }
    }
}