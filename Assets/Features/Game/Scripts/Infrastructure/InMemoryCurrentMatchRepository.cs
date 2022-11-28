using System.Linq;
using Features.Game.Scripts.Domain;
using Features.Match.Domain;
using Infrastructure.Data;

namespace Features.Game.Scripts.Infrastructure
{
    public class InMemoryCurrentMatchRepository : ICurrentMatchRepository
    {
        private GameMatch _gameMatch;
        public GameMatch Get() => _gameMatch;

        public void Set(GameMatch gameMatch) => _gameMatch = gameMatch;
        public void Set(Hand hand)
        {
            _gameMatch.Hand = hand;
        }

        public void SetRounds(Round round)
        {
            var rounds = _gameMatch.Board.Rounds;
            var savedRound = rounds.FirstOrDefault(x => x.RoundNumber == round.RoundNumber);
            if (round.RoundNumber == 0)
                return;
            rounds.Remove(savedRound);
            
            rounds.Add( round);
            _gameMatch.Board.Rounds = rounds.OrderBy(x => x.RoundNumber).ToList();
        }
    }
}