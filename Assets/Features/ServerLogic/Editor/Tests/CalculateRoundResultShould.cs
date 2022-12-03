using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using NSubstitute;
using NUnit.Framework;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Users.Domain;
using UniRx;

namespace Features.ServerLogic.Editor.Tests
{
    public class CalculateRoundResultShould
    {
        private const string UserIdOne = "Id";
        private const string UserIdTwo = "Id-2";
        private const string UserNameOne = "userName";
        private const string UserNameTwo = "userName-2";
        private CalculateRoundResult _calculateRoundResultShould;


        [SetUp]
        public void Setup()
        {
            _calculateRoundResultShould = new CalculateRoundResult();
        }
        
        static RoundResultTestCaseSource[] _roundsCases =
        {
            new RoundResultTestCaseSource {IsTie = false, ExpectedWinner = UserIdOne},
            new RoundResultTestCaseSource {IsTie = false, ExpectedWinner = UserIdTwo}
        };

        [TestCaseSource(nameof(_roundsCases))]
        [Test]
        public void Test(RoundResultTestCaseSource roundCase)
        {
            var users = new List<string>()
            {
                UserIdOne,
                UserNameTwo
            };
            var round = ARound(users);
            
            WhenExecute(round);
            ThenRoundWinnerIs();
            void ThenRoundWinnerIs()
            {
                Assert.AreEqual(1,round.PlayerWinner.Count);
                Assert.IsTrue(round.PlayerWinner.Any(x=>x.UserName == roundCase.ExpectedWinner));
            }
        }

        private void WhenExecute(Round round) => _calculateRoundResultShould.Execute(round);

        private KeyValuePair<string, PlayerCard> APlayerOneInfo(PlayerCard withPlayerCard=null) =>
            new(UserIdOne,withPlayerCard ?? 
                new PlayerCard()
                {
                    UpgradeCard = new UpgradeCard(),
                    UnitCard = new UnitCard(),
                });
        
        private KeyValuePair<string, PlayerCard> APlayerTwoInfo(PlayerCard withPlayerCard=null) =>
            new(UserIdTwo,withPlayerCard??
                new PlayerCard()
                {
                    UpgradeCard = new UpgradeCard(),
                    UnitCard = new UnitCard(),
                });

        private Dictionary<string, PlayerCard> APlayerCards(KeyValuePair<string, PlayerCard> withPlayerOne, KeyValuePair<string, PlayerCard> withPlayerTwo) =>
            new()
            {
                {withPlayerOne.Key, withPlayerOne.Value},
                {withPlayerTwo.Key, withPlayerTwo.Value},
            };

        private  Round ARound(List<string> withUsers, Dictionary<string, PlayerCard> withPlayerCards = null,UpgradeCard withRoundUpgradeCard = null )
        {
            return new Round(withUsers)
            {
                PlayerCards = withPlayerCards ?? APlayerCards (APlayerOneInfo(), APlayerTwoInfo()),
                RoundUpgradeCard = withRoundUpgradeCard ?? new UpgradeCard(),
            };
        }
    }

    public class RoundResultTestCaseSource
    {
        internal bool IsTie;
        internal string ExpectedWinner;
        internal Round round;
    }
}