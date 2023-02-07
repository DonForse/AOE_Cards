using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class FurorCelticaApplyEffectPostUnitStrategyShould
    {
        private const string MatchId = "MatchId";
        private const string UserId = "UserId";
        private FurorCelticaApplyEffectPostUnitStrategy _strategy;
        private IMatchesRepository _matchesRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _strategy = new FurorCelticaApplyEffectPostUnitStrategy(_matchesRepository);
        }

        [Test]
        public void ApplyFurorCelticaEffect()
        {
            var card = UpgradeCardMother.CreateFakeFurorCeltica();
            var siege = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.SiegeUnit });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = siege,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            _matchesRepository.Get(MatchId).Returns(match);


            _strategy.Execute(card, MatchId,UserId);
            _matchesRepository.Received(1).Update(match);
            Assert.IsTrue(match.Board.PlayersHands[UserId].UnitsCards.Contains(siege));
        }
        
        [Test]
        public void NotApplyFurorCelticaEffectIfNotFurorCeltica()
        {
            var card = UpgradeCardMother.Create();
            var siege = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.SiegeUnit });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = siege,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            _matchesRepository.Get(MatchId).Returns(match);


            _strategy.Execute(card, MatchId,UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.IsFalse(match.Board.PlayersHands[UserId].UnitsCards.Contains(siege));
        }
        
        [Test]
        public void NotApplyFurorCelticaEffectIfUnitNotSiege()
        {
            var card = UpgradeCardMother.CreateFakeFurorCeltica();
            var siege = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.Monk });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = siege,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            _matchesRepository.Get(MatchId).Returns(match);


            _strategy.Execute(card, MatchId,UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.IsFalse(match.Board.PlayersHands[UserId].UnitsCards.Contains(siege));
        }

        private static ServerMatch AServerMatch(PlayerCard playerCard, UpgradeCard roundUpgrade)
        {
            return ServerMatchMother.Create(MatchId,
                new List<User>
                {
                    UserMother.Create(UserId), UserMother.Create(UserId + "2")
                },
                BoardMother.Create(DeckMother.CreateWithRandomCards(),
                    RoundMother.Create(new List<string>()
                        {
                            UserId, UserId+"2"
                        },new Dictionary<string, PlayerCard>()
                        {
                            {UserId, playerCard},
                            {UserId+"2", new PlayerCard()}
                        }, 
                        withRoundUpgradeCard: roundUpgrade
                    ), new Dictionary<string, Hand>
                    {
                        {
                            UserId, new Hand { UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}
                        }, {UserId+"2", new Hand { UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}}
                    },
                    new List<Round>()));
        }
        
    }
}