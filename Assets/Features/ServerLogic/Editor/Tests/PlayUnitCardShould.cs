﻿using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;
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
        private ICalculateRoundResult _calculateRoundResult;
        private ICalculateMatchResult _calculateMatchResult;
        private ICreateNewRound _createNewRound;
        private IApplyEffectPostUnit _applyEffectPostUnit;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _cardRepository = Substitute.For<ICardRepository>();
            _calculateRoundResult = Substitute.For<ICalculateRoundResult>();
            _calculateMatchResult = Substitute.For<ICalculateMatchResult>();
            _createNewRound = Substitute.For<ICreateNewRound>();
            _applyEffectPostUnit = Substitute.For<IApplyEffectPostUnit>();
            _playUnitCard = new PlayUnitCard(_matchesRepository, _cardRepository, _calculateRoundResult, _calculateMatchResult, _createNewRound, _applyEffectPostUnit);
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
            GivenServerMatch(AServerMatch(null));
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
                    withBoard: BoardMother.Create(withCurrentRound: RoundMother.Create(new []{UserId, UserId+"2"})));
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
                    withBoard: BoardMother.Create(withCurrentRound: RoundMother.Create(new[] {UserId, UserId + "2"},
                            withPlayerCards: new Dictionary<string, PlayerCard>()
                            {
                                {UserId, new PlayerCard() {UnitCard = UnitCardMother.Create("Some Card")}}
                            })
                    ));
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
                    withBoard: BoardMother.Create(withCurrentRound: RoundMother.Create(new []{UserId, UserId+"2"}, 
                            new Dictionary<string, PlayerCard> { {UserId, new PlayerCard()}},
                            withRoundState: RoundState.Upgrade)));
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
                    withBoard: BoardMother.Create(withCurrentRound:
                        RoundMother.Create(new []{UserId, UserId+"2"}, 
                            new Dictionary<string, PlayerCard> { {UserId, new PlayerCard()}},
                            withRoundState: RoundState.Upgrade), 
                        withPlayerHands: new Dictionary<string, Hand> { {UserId, new Hand()}}));
            }
        }
        
        [Test]
        public void SetUnitCardPlayed()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);            
            GivenServerMatch(serverMatch);
            WhenExecute();
            
            Assert.IsFalse(serverMatch.Board.PlayersHands[UserId].UnitsCards.Any(card => card.cardName == CardName));
            Assert.IsTrue(serverMatch.Board.CurrentRound.PlayerCards[UserId].UnitCard.cardName == CardName);
        }

        [Test]
        public void RemoveUnitCardPlayedFromHand()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            GivenServerMatch(serverMatch);
            GivenCardPlayed();
            
            Assert.IsTrue(serverMatch.Board.PlayersHands[UserId].UnitsCards.Any(card => card.cardName == CardName));

            WhenExecute();
            
            Assert.IsFalse(serverMatch.Board.PlayersHands[UserId].UnitsCards.Any(card => card.cardName == CardName));
            Assert.IsTrue(serverMatch.Board.CurrentRound.PlayerCards[UserId].UnitCard.cardName == CardName);        
        }

        [Test]
        public void DoNotRemoveUnitCardPlayedFromHandIfUnitIsVillager()
        {
            var cardName = "Villager";
            var card = GivenVillagerCardPlayed();
            var serverMatch = AServerMatch(card);
            GivenServerMatch(serverMatch);

            Assert.IsTrue(serverMatch.Board.PlayersHands[UserId].UnitsCards.Any(card => card.cardName == cardName));

            WhenExecuteWithVillagerCard();
            
            Assert.IsTrue(serverMatch.Board.PlayersHands[UserId].UnitsCards.Any(card => card.cardName == cardName));
            Assert.IsTrue(serverMatch.Board.CurrentRound.PlayerCards[UserId].UnitCard.cardName == cardName);
        }

        [Test]
        public void ApplyUpgradePostUnitPlayedEffects()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            GivenServerMatch(serverMatch);
            WhenExecute();
            _applyEffectPostUnit.Received(1).Execute(MatchId, UserId);
            }

        [Test]
        public void CreateNewRoundIfMatchIsNotFinished()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UpgradeCard = UpgradeCardMother.Create();
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UnitCard = UnitCardMother.Create();

            GivenMatchIsNotFinished();
            GivenServerMatch(serverMatch);
            WhenExecute();
            ThenCreateRound();

            void ThenCreateRound() => _createNewRound.Received(1).Execute(MatchId);
        }
        
        [Test]
        public void DoNotCreateNewRoundIfMatchIsFinished()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UpgradeCard = UpgradeCardMother.Create();
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UnitCard = UnitCardMother.Create();

            GivenMatchIsFinished();
            GivenServerMatch(serverMatch);
            WhenExecute();
            ThenCreateRound();

            void ThenCreateRound() => _createNewRound.DidNotReceive().Execute(Arg.Any<string>());
        }

        [Test]
        public void DetermineRoundWinner()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UpgradeCard = UpgradeCardMother.Create();
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UnitCard = UnitCardMother.Create();

            GivenServerMatch(serverMatch);
            WhenExecute();
            ThenCalculateRoundResultIsCalled();

            void ThenCalculateRoundResultIsCalled() => _calculateRoundResult.Received(1).Execute(MatchId);
        }

        [Test]
        public void DetermineMatchWinner()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UpgradeCard = UpgradeCardMother.Create();
            serverMatch.Board.CurrentRound.PlayerCards[UserId +"2"].UnitCard = UnitCardMother.Create();

            GivenServerMatch(serverMatch);
            WhenExecute();
            ThenCreateRound();

            void ThenCreateRound() => _calculateMatchResult.Received(1).Execute(MatchId);
        }



        [Test]
        public void UpdatesMatchRepository()
        {
            var card = GivenCardPlayed();
            var serverMatch = AServerMatch(card);
            GivenServerMatch(serverMatch);
            WhenExecute();
            _matchesRepository.Received(1).Update(serverMatch);
        }


        private UnitCard GivenCardPlayed()
        {
            var card = UnitCardMother.Create(CardName);
            _cardRepository.GetUnitCard(CardName).Returns(card);
            return card;
        }

        private UnitCard GivenVillagerCardPlayed()
        {
            var card = UnitCardMother.Create("Villager");
            _cardRepository.GetUnitCard("Villager").Returns(card);
            return card;
        }
        private void GivenMatchIsNotFinished() => _calculateMatchResult.Execute(MatchId).Returns(false);
        private void GivenMatchIsFinished() => _calculateMatchResult.Execute(MatchId).Returns(true);
        void GivenServerMatch(ServerMatch serverMatch) => _matchesRepository.Get(MatchId).Returns(serverMatch);

        private static ServerMatch AServerMatch(UnitCard cardInHand)
        {
            return ServerMatchMother.Create(MatchId,
                withUsers: new List<User>() {UserMother.Create(UserId), UserMother.Create(UserId + "2")},
                withBoard: BoardMother.Create(
                    withPlayerHands: new Dictionary<string, Hand>()
                    {
                        {
                            UserId, new Hand()
                            {
                                UnitsCards = new List<UnitCard>() {cardInHand},
                                UpgradeCards = new List<UpgradeCard>()
                            }
                        },
                        {
                            UserId + "2", new Hand()
                            {
                                UnitsCards = new List<UnitCard>() { },
                                UpgradeCards = new List<UpgradeCard>()
                            }
                        }
                    },
                    withRoundsPlayed: new List<Round>()
                    {
                        RoundMother.Create(new[] {UserId, UserId + "2"},
                            withPlayerCards: new Dictionary<string, PlayerCard>()
                            {
                                {UserId, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create()}},
                                {UserId + 2, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create()}}
                            }, withRoundState: RoundState.Finished,
                            withRoundUpgradeCard: UpgradeCardMother.Create())
                    },
                    withCurrentRound: RoundMother.Create(
                        new[] {UserId, UserId + "2"},
                        withPlayerCards: new Dictionary<string, PlayerCard>()
                        {
                            {UserId, new PlayerCard() {UpgradeCard = UpgradeCardMother.Create()}},
                            {UserId + 2, new PlayerCard()}
                        },
                        withRoundState: RoundState.Unit,
                        withRoundUpgradeCard: UpgradeCardMother.Create()),
                    withDeck:DeckMother.CreateWithRandomCards(10,10)
                ));
        }
        
        private void WhenExecute() => _playUnitCard.Execute(MatchId, UserId, CardName);
        private void WhenExecuteWithVillagerCard() => _playUnitCard.Execute(MatchId, UserId, "Villager");

        private void ThenThrowsError(TestDelegate code) => Assert.Throws<ApplicationException>(code);

    }
}