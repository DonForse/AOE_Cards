using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class CreateRoundShould
    {
        private CreateNewRound _createNewRound;
        private IMatchesRepository _matchesRepository;
        private const string MatchId = "MatchId";


        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _createNewRound = new CreateNewRound(_matchesRepository);
        }

        [Test]
        public void ReturnANewRoundWithNoReroll()
        {
            var previousCurrentRound = RoundMother.Create();
            var match = ServerMatchMother.Create(withBoard:
                BoardMother.Create(
                    withCurrentRound:previousCurrentRound,
                    withRoundsPlayed: new List<Round>()));
            _matchesRepository.Get(MatchId).Returns(match);
            _createNewRound.Execute(MatchId);


            Assert.AreEqual( 1, match.Board.RoundsPlayed.Count);
            Assert.AreEqual(previousCurrentRound, match.Board.RoundsPlayed.Last());
            Assert.AreEqual(RoundState.Upgrade,match.Board.CurrentRound.RoundState);
        }
        
        [Test]
        public void ReturnANewRoundWithRerollWhenNewRoundIsTheFirst()
        {
            var previousCurrentRound = (Round)null;
            var match = ServerMatchMother.Create(withBoard:
                BoardMother.Create(
                    withCurrentRound:previousCurrentRound,
                    withRoundsPlayed: new List<Round>()));
            _matchesRepository.Get(MatchId).Returns(match);
            _createNewRound.Execute(MatchId);


            Assert.AreEqual( 0, match.Board.RoundsPlayed.Count);
            Assert.AreEqual(RoundState.Reroll,match.Board.CurrentRound.RoundState);
        }
        
        [Test]
        public void ReturnANewRoundWithRerollWhenNewRoundIsTheThird()
        {
            var previousCurrentRound = RoundMother.Create();
            var match = ServerMatchMother.Create(withBoard:
                BoardMother.Create(
                    withCurrentRound:previousCurrentRound,
                    withRoundsPlayed: new List<Round>{RoundMother.Create()}));
            _matchesRepository.Get(MatchId).Returns(match);
            _createNewRound.Execute(MatchId);


            Assert.AreEqual( 2, match.Board.RoundsPlayed.Count);
            Assert.AreEqual(previousCurrentRound, match.Board.RoundsPlayed.Last());
            Assert.AreEqual(RoundState.Reroll,match.Board.CurrentRound.RoundState);
        }
        [Test]
        public void ReturnANewRoundWithRerollWhenNewRoundIsTheSixth()
        {
            var previousCurrentRound = RoundMother.Create();
            var match = ServerMatchMother.Create(withBoard:
                BoardMother.Create(
                    withCurrentRound:previousCurrentRound,
                    withRoundsPlayed: new List<Round>{
                        RoundMother.Create(),RoundMother.Create(),
                        RoundMother.Create(),RoundMother.Create()}));
            _matchesRepository.Get(MatchId).Returns(match);
            _createNewRound.Execute(MatchId);

            Assert.AreEqual( 5, match.Board.RoundsPlayed.Count);
            Assert.AreEqual(previousCurrentRound, match.Board.RoundsPlayed.Last());
            Assert.AreEqual(RoundState.Reroll,match.Board.CurrentRound.RoundState);
        }
    }
}