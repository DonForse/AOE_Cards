using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class ApplyEffectPostUnitShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private ApplyEffectPostUnit _applyEffectPostUnit;
        private IGetMatch _getMatch;

        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _applyEffectPostUnit = new ApplyEffectPostUnit(_getMatch);
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
            
            _getMatch.Execute(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Assert.IsTrue(match.Board.PlayersHands[UserId].UnitsCards.Contains(siege));
        }
        
        [Test]
        public void ApplyMadrasahEffect()
        {
            var card = UpgradeCardMother.CreateFakeMadrasah();
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.Monk });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            
            _getMatch.Execute(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Assert.IsTrue(match.Board.PlayersHands[UserId].UnitsCards.Contains(monk));
        }
        
        [Test]
        public void ApplyMadrasahEffectWhenRoundUpgrade()
        {
            var card = UpgradeCardMother.CreateFakeMadrasah();
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.Monk });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = UpgradeCardMother.Create()
            }, card);
            
            _getMatch.Execute(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Assert.IsTrue(match.Board.PlayersHands[UserId].UnitsCards.Contains(monk));
        }

        [Test]
        public void NotApplyMadrasahEffectWhenNotPlayed()
        {
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.Monk });
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = UpgradeCardMother.Create()
            }, UpgradeCardMother.Create());
            
            _getMatch.Execute(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Assert.IsFalse(match.Board.PlayersHands[UserId].UnitsCards.Contains(monk));
        }
        
        [Test]
        public void ApplyMadrasahEffectWhenUpgradePlayerPreviousRound()
        {
            var card = UpgradeCardMother.CreateFakeMadrasah();
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype> { Archetype.Monk });
            var match = AServerMatchWithPreviousRound(new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = UpgradeCardMother.Create()
            },
                new PlayerCard()
                {
                    UnitCard = monk,
                    UpgradeCard = card
                });
            
            _getMatch.Execute(MatchId).Returns(match);
            _applyEffectPostUnit.Execute(MatchId, UserId);
            
            Assert.IsTrue(match.Board.PlayersHands[UserId].UnitsCards.Contains(monk));
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
        
        private static ServerMatch AServerMatchWithPreviousRound(PlayerCard playerCard,PlayerCard previousPlayerCard)
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
                        withRoundUpgradeCard: UpgradeCardMother.Create()
                    ), new Dictionary<string, Hand>
                    {
                        {
                            UserId, new Hand { UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}
                        }, {UserId+"2", new Hand { UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}}
                    },
                    new List<Round>()
                    {
                        RoundMother.Create(new List<string>()
                        {
                            UserId, UserId+"2"
                        },new Dictionary<string, PlayerCard>()
                        {
                            {UserId, previousPlayerCard},
                            {UserId+"2", new PlayerCard()}
                        }, 
                        withRoundUpgradeCard: UpgradeCardMother.Create())
                    }));
        }
    }
}