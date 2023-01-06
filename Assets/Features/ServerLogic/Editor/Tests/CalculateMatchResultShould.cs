using System.Collections.Generic;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class CalculateMatchResultShould
    {
        private const string MatchId = "MatchId";
        private CalculateMatchResult _calculateMatchResult;
        private IGetMatch _getMatch;


        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _calculateMatchResult = new CalculateMatchResult(_getMatch);
        }

        [Test]
        public void ReturnFalseWhenNotEnoughRoundsWonByAPlayer()
        {
            var match = AMatch();
            _getMatch.Execute(MatchId).Returns(match);
            var result = _calculateMatchResult.Execute(MatchId);
            Assert.AreEqual(false, result);
            Assert.IsNull(match.MatchWinner);
            Assert.AreEqual(false, match.IsFinished);
            Assert.AreEqual(false, match.IsTie);
            Assert.AreEqual(RoundState.Reroll, match.Board.CurrentRound.RoundState);
            /*
             *var winnersGrouped = serverMatch.Board.RoundsPlayed.SelectMany(r => r.PlayerWinner).GroupBy(c => c.Id);
            //TODO: Coul be a tie
            serverMatch.IsFinished = winnersGrouped.Any(w => w.Count() >= 4);
            if (!serverMatch.IsFinished)
                return false;
    
            serverMatch.IsTie = winnersGrouped.Count(w => w.Count() >= 4) > 1;
    
            serverMatch.MatchWinner = winnersGrouped.FirstOrDefault(w => w.Count() >= 4).First();
            if (serverMatch.IsFinished)
                serverMatch.Board.CurrentRound.ChangeRoundState(RoundState.GameFinished);
            return true;
             * 
             */
        }

        [Test]
        public void ReturnTrueWhenEnoughRoundsWonByAPlayer()
        {
            var winner = UserMother.Create("UserId");
            var match = AMatchWonByUser(winner);
            _getMatch.Execute(MatchId).Returns(match);
            var result = _calculateMatchResult.Execute(MatchId);
            Assert.AreEqual(true, result);
            Assert.AreEqual(winner, match.MatchWinner);
            Assert.AreEqual(true, match.IsFinished);
            Assert.AreEqual(false, match.IsTie);
            Assert.AreEqual(RoundState.GameFinished, match.Board.CurrentRound.RoundState);
        }
        
        [Test]
        public void SetTieToTrueWhenPlayersTie()
        {
            var match = AMatchTiedByUser();
            _getMatch.Execute(MatchId).Returns(match);
            var result = _calculateMatchResult.Execute(MatchId);
            Assert.AreEqual(true, result);
            Assert.AreEqual(true, match.IsFinished);
            Assert.AreEqual(true, match.IsTie);
            Assert.AreEqual(RoundState.GameFinished, match.Board.CurrentRound.RoundState);
        }

        public ServerMatch AMatch() => ServerMatchMother.Create(MatchId,
            new List<User> {UserMother.Create("UserId"), UserMother.Create("UserId2")},
            BoardMother.Create(withCurrentRound: RoundMother.Create(), withRoundsPlayed: new List<Round>()));
        
        public ServerMatch AMatchWonByUser(User winnerUser) => ServerMatchMother.Create(MatchId,
            new List<User> {winnerUser, UserMother.Create("UserId2")},
            BoardMother.Create(withCurrentRound: RoundMother.Create(withPlayersWinner:new List<User> {winnerUser}), 
                withRoundsPlayed: new List<Round>
                {
                    RoundMother.Create(withPlayersWinner:new List<User> {winnerUser}),
        RoundMother.Create(withPlayersWinner:new List<User> {winnerUser}),
        RoundMother.Create(withPlayersWinner:new List<User> {winnerUser}),
                }));
        
        public ServerMatch AMatchTiedByUser()
        {
            var user1 = UserMother.Create("UserId");
            var user2 = UserMother.Create("UserId2");
            return ServerMatchMother.Create(MatchId,
                new List<User> {user1,user2},
                BoardMother.Create(withCurrentRound: RoundMother.Create(withPlayersWinner: new List<User> {user1, user2}),
                    withRoundsPlayed: new List<Round>
                    {
                        RoundMother.Create(withPlayersWinner: new List<User> {user1}),
                        RoundMother.Create(withPlayersWinner: new List<User> {user1}),
                        RoundMother.Create(withPlayersWinner: new List<User> {user1}),
                        RoundMother.Create(withPlayersWinner: new List<User> {user2}),
                        RoundMother.Create(withPlayersWinner: new List<User> {user2}),
                        RoundMother.Create(withPlayersWinner: new List<User> {user2}),
                    }));
        }
    }
}