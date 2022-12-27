using System;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
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
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
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
            GivenCardDoesNotExists();
            ThenThrowsError(WhenExecute);

            void GivenCardDoesNotExists() => _cardRepository.GetUnitCard(CardName).Returns((UnitCard)null);
        }

        [Test]
        public void ThrowsWhenPlayerNotInMatch()
        {
            GivenServerMatchWithNoPlayerCards();
            GivenCardPlayed();
            ThenThrowsError(WhenExecute);
            
            void GivenServerMatchWithNoPlayerCards() => _matchesRepository.Get(MatchId).Returns(AServerMatchWithNoPlayerCards());
            ServerMatch AServerMatchWithNoPlayerCards()
            {
                return ServerMatchMother.Create(MatchId,
                    withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
                    {
                        RoundMother.Create(new []{UserId, UserId+"2"})
                    }));
            }
        }

        [Test]
        public void ThrowsWhenUnitCardAlreadyPlayed()
        {
            GivenServerMatchWithUnitCardAlreadyPlayed();
            GivenCardPlayed();
            ThenThrowsError(WhenExecute);
            
            void GivenServerMatchWithUnitCardAlreadyPlayed() => _matchesRepository.Get(MatchId).Returns(AServerMatchWithUnitCardAlreadyPlayed());
            ServerMatch AServerMatchWithUnitCardAlreadyPlayed()
            {
                return ServerMatchMother.Create(MatchId,
                    withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
                    {
                        RoundMother.Create(new []{UserId, UserId+"2"}, withPlayerCards:new Dictionary<string, PlayerCard>()
                        {
                            {UserId, new PlayerCard(){UnitCard = UnitCardMother.Create("Some Card")}}
                        })
                    }));
            }
        }

        [Test]
        public void ThrowsWhenRoundNotInUnitPhase()
        {
            GivenServerMatchWithRoundInUpgrade();
            GivenCardPlayed();
            ThenThrowsError(WhenExecute);
            
            void GivenServerMatchWithRoundInUpgrade() => _matchesRepository.Get(MatchId).Returns(AServerMatchWithUpgradePhase());
            ServerMatch AServerMatchWithUpgradePhase()
            {
                return ServerMatchMother.Create(MatchId,
                    withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
                    {
                        RoundMother.Create(new []{UserId, UserId+"2"}, 
                            new Dictionary<string, PlayerCard> { {UserId, new PlayerCard()}},
                            withRoundState: RoundState.Upgrade)
                    }));
            }
        }

        [Test]
        public void ThrowsWhenUnitCardNotInHand()
        {
            GivenServerMatchWithCardNotInHand();
            GivenCardPlayed();
            ThenThrowsError(WhenExecute);
            
            void GivenServerMatchWithCardNotInHand() => _matchesRepository.Get(MatchId).Returns(AServerMatchWithCardNotInHand());
            ServerMatch AServerMatchWithCardNotInHand()
            {
                return ServerMatchMother.Create(MatchId,
                    withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
                    {
                        RoundMother.Create(new []{UserId, UserId+"2"}, 
                            new Dictionary<string, PlayerCard> { {UserId, new PlayerCard()}},
                            withRoundState: RoundState.Upgrade)
                    }, withPlayerHands: new Dictionary<string, Hand> { {UserId, new Hand()}}));
            }
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



        private void GivenCardPlayed() => _cardRepository.GetUnitCard(CardName).Returns(UnitCardMother.Create(CardName));
        void GivenServerMatch() => _matchesRepository.Get(MatchId).Returns(AServerMatch());

        private static ServerMatch AServerMatch()
        {
            return ServerMatchMother.Create(MatchId,
                withBoard: BoardMother.Create(withRoundsPlayed: new List<Round>()
            {
                RoundMother.Create(new []{UserId, UserId+"2"}, withPlayerCards:new Dictionary<string, PlayerCard>()
                {
                    {UserId, new PlayerCard()},
                    {UserId+2, new PlayerCard()}
                })
            }));
        }
        
        private void WhenExecute() => _playUnitCard.Execute(MatchId, UserId, CardName);
        private void ThenThrowsError(TestDelegate code) => Assert.Throws<ApplicationException>(code);

    }
}