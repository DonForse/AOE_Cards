using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class PersianTcPreCalculusCardStrategyShould
    {
        private const string UserId = "UserId";
        private const string MatchId = "MatchId";
        private PersianTcPreCalculusCardStrategy _strategy;
        private IMatchesRepository _matchesRepository;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;

        private static IRoundResultTestCaseSource[] _roundsCases =
        {
            new WinWithPersianTC(),
        };

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _getPlayerPlayedUpgradesInMatch = Substitute.For<IGetPlayerPlayedUpgradesInMatch>();
            _strategy = new PersianTcPreCalculusCardStrategy(_getPlayerPlayedUpgradesInMatch, _matchesRepository);
        }

        [Test]
        public void ApplyPersianTcEffect()
        {
            var card = UpgradeCardMother.CreateFakePersianTC();
            var villUpgradeOne = UpgradeCardMother.Create(withArchetypes: new List<Archetype>()
            {
                Archetype.Villager
            }, withBasePower: 5);
            var villUpgradeTwo = UpgradeCardMother.Create(withArchetypes: new List<Archetype>()
            {
                Archetype.Villager
            }, withBasePower: 10);
            
            var villUpgradeThree = UpgradeCardMother.Create(withArchetypes: new List<Archetype>()
            {
                Archetype.Villager
            }, withBasePower: 20);
            
            var villager = UnitCardMother.Create(withArchetypes: new List<Archetype> {Archetype.Villager});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = villager,
                UpgradeCard = card
            }, UpgradeCardMother.Create());

            var upgrades = new List<UpgradeCard>
                {card, villUpgradeOne, villUpgradeTwo, villUpgradeThree};

            _matchesRepository.Get(MatchId).Returns(match);
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserId).Returns(upgrades);
            
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.Received(1).Update(match);
            Assert.AreEqual(35, card.basePower);
        }

        [Test]
        public void NotApplyPersianTcEffectIfNotPersianTc()
        {
            var card = UpgradeCardMother.Create();
            var villager = UnitCardMother.Create(withArchetypes: new List<Archetype> {Archetype.Villager});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = villager,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            
            var upgrades = new List<UpgradeCard> {card};
            _matchesRepository.Get(MatchId).Returns(match);
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserId).Returns(upgrades);
            _matchesRepository.Get(MatchId).Returns(match);
            _strategy.Execute(card, MatchId, UserId);
           
            _matchesRepository.DidNotReceive().Update(match);
            Assert.AreEqual(0, card.basePower);

        }

        [Test]
        public void NotApplyPersianTcEffectIfUnitNotVillager()
        {
            var card = UpgradeCardMother.CreateFakePersianTC();
            var siege = UnitCardMother.Create(withArchetypes: new List<Archetype> {Archetype.SiegeUnit});
            var match = AServerMatch(new PlayerCard()
            {
                UnitCard = siege,
                UpgradeCard = card
            }, UpgradeCardMother.Create());
            
            var upgrades = new List<UpgradeCard> {card};
            _matchesRepository.Get(MatchId).Returns(match);
            _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserId).Returns(upgrades);
            _matchesRepository.Get(MatchId).Returns(match);
            
            _strategy.Execute(card, MatchId, UserId);
            
            _matchesRepository.DidNotReceive().Update(match);
            Assert.AreEqual(0, card.basePower);
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
                            UserId, UserId + "2"
                        }, new Dictionary<string, PlayerCard>()
                        {
                            {UserId, playerCard},
                            {UserId + "2", new PlayerCard()}
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