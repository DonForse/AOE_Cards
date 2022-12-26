using System;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PlayUnitCardShould
    {
        private const string CardName = "UnitCard";
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        
        private IMatchesRepository _matchesRepository;
        private ICardRepository _cardRepository;
        private PlayUnitCard _playUnitCard;

        [SetUp]
        public void Setup()
        {
            _playUnitCard = new PlayUnitCard(_matchesRepository, _cardRepository);
        }

        [Test]
        public void ThrowsErrorWhenMatchNotExists()
        {
            GivenMatchDoesNotExists();
            ThenThrowsError(WhenExecute);
            
            void GivenMatchDoesNotExists() => _matchesRepository.Get(MatchId).Returns((ServerMatch) null);
        }
        
        [Test]
        public void ThrowsErrorWhenCardNotExists()
        {
            GivenServerMatch();
            ThenThrowsError(WhenExecute);
            Assert.Fail();
        }

        [Test]
        public void ThrowsWhenPlayerNotInMatch()
        {
            Assert.Fail();
        }
        
        [Test]
        public void ThrowsWhenUnitCardAlreadyPlayed()
        {
            Assert.Fail();
        }

        [Test]
        public void ThrowsWhenRoundNotInUnitPhase()
        {
            Assert.Fail();
        }
        
        [Test]
        public void ThrowsWhenUnitCardNotInHand()
        {
            Assert.Fail();
        }

        [Test]
        public void ApplyUpgradePreUnitPlayedEffects()
        {
        }

        [Test]
        public void SetUnitCardPlayed()
        {
            Assert.Fail();
        }
        
        [Test]
        public void RemoveUnitCardPlayedFromHand()
        {
            Assert.Fail();
        }

        [Test]
        public void DoNotRemoveUnitCardPlayedFromHandIfUnitIsVillager()
        {
            Assert.Fail();
        }
        
        [Test]
        public void ApplyUpgradePostUnitPlayedEffects()
        {
        }
        
        [Test]
        public void ChangeRoundPhaseToFinishedIfAllPlayersPlayedUnit()
        {
            Assert.Fail();
        }
        
        [Test]
        public void DetermineRoundWinner()
        {
            Assert.Fail();
        }

        [Test]
        public void DetermineMatchWinner()
        {
            Assert.Fail();
        }

        [Test]
        public void CreateNewRoundIfMatchIsNotFinished()
        {
            Assert.Fail();
        }
        
        
        [Test]
        public void UpdatesMatchRepository()
        {
            Assert.Fail();
        }


        void GivenServerMatch() => _matchesRepository.Get(MatchId).Returns(ServerMatchMother.Create(MatchId));
        private void WhenExecute() => _playUnitCard.Execute(MatchId, UserId, CardName);
        private void ThenThrowsError(TestDelegate code) => Assert.Throws<ApplicationException>(code);

    }
}