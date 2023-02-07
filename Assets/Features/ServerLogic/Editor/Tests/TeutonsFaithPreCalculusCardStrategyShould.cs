using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;

namespace Features.ServerLogic.Editor.Tests
{
    public class TeutonsFaithPreCalculusCardStrategyShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private TeutonsFaithPreCalculusCardStrategy _strategy;
        private IMatchesRepository _matchesRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _strategy = new TeutonsFaithPreCalculusCardStrategy(_matchesRepository);
        }
        
        [Test]
        public void ApplyTeutonsFaith()
        {
            var card = UpgradeCardMother.CreateFakeTeutonsFaithCard();
            var cavalry = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Cavalry});
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Monk});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = cavalry,
                UpgradeCard = card
            }, new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = null
            },UpgradeCardMother.Create());
            
            _matchesRepository.Get(MatchId).Returns(match);
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.Received(1).Update(match);
            Assert.AreEqual(1000, card.basePower);
        }

        [Test]
        public void NotApplyPersianTcEffectIfNotTeutonsFaith()
        {
            var card = UpgradeCardMother.Create();
            var cavalry = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Cavalry});
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Monk});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = cavalry,
                UpgradeCard = card
            }, new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = null
            },UpgradeCardMother.Create());
            
            _matchesRepository.Get(MatchId).Returns(match);
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.AreEqual(0, card.basePower);
        }

        [Test]
        public void NotApplyTeutonsFaithEffectIfUnitCavalry()
        {
            var card = UpgradeCardMother.CreateFakeTeutonsFaithCard();
            var cavalry = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Villager});
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Monk});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = cavalry,
                UpgradeCard = card
            }, new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = null
            },UpgradeCardMother.Create());
            
            _matchesRepository.Get(MatchId).Returns(match);
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.AreEqual(0, card.basePower);
        }
        
        [Test]
        public void NotApplyTeutonsFaithEffectIfRivalIsNotMonk()
        {
            var card = UpgradeCardMother.CreateFakeTeutonsFaithCard();
            var cavalry = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.Cavalry});
            var monk = UnitCardMother.Create(withArchetypes: new List<Archetype>() {Archetype.SiegeUnit});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = cavalry,
                UpgradeCard = card
            }, new PlayerCard()
            {
                UnitCard = monk,
                UpgradeCard = null
            },UpgradeCardMother.Create());
            
            _matchesRepository.Get(MatchId).Returns(match);
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.AreEqual(0, card.basePower);
        }

        private static ServerMatch AServerMatch(PlayerCard playerCard, PlayerCard rivalPlayerCard, UpgradeCard roundUpgrade)
        {
            return ServerMatchMother.Create(MatchId,
                new List<User>
                {
                    UserMother.Create(UserId), UserMother.Create(UserId + "2")
                },
                BoardMother.Create(DeckMother.CreateWithRandomCards(),
                    RoundMother.Create(new List<string>()
                        {
                            UserId, UserId + "2"
                        }, new Dictionary<string, PlayerCard>()
                        {
                            {UserId, playerCard},
                            {UserId + "2",rivalPlayerCard}
                        },
                        withRoundUpgradeCard: roundUpgrade
                    ), new Dictionary<string, Hand>
                    {
                        {
                            UserId, new Hand {UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}
                        },
                        {
                            UserId + "2",
                            new Hand {UnitsCards = new List<UnitCard>(), UpgradeCards = new List<UpgradeCard>()}
                        }
                    },
                    new List<Round>()));
        }
    }
}