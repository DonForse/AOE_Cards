using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Users.Domain;
using NSubstitute;
using NUnit.Framework;

namespace Features.ServerLogic.Editor.Tests
{
    public class CalculateRoundResultShould
    {
        private const string UserIdOne = "Id";
        private const string UserIdTwo = "Id-2";
        private const string MatchId = "MATCH-ID";
        private CalculateRoundResult _calculateRoundResultShould;
        private IGetMatch _getMatch;
        private IGetPlayerPlayedUpgradesInMatch _getPlayerPlayedUpgradesInMatch;


        [SetUp]
        public void Setup()
        {
            _getMatch = Substitute.For<IGetMatch>();
            _getPlayerPlayedUpgradesInMatch = Substitute.For<IGetPlayerPlayedUpgradesInMatch>();
            _calculateRoundResultShould = new CalculateRoundResult(_getMatch,_getPlayerPlayedUpgradesInMatch);
        }

        static IRoundResultTestCaseSource[] _roundsCases =
        {
            new EqualCardsTieRoundResult(),
            new UserOneWinRoundResult(),
            new UserTwoWinRoundResult(),
            new UserOneWinRoundResultUpgradeCard(),
            new UserTwoWinRoundResultUpgradeCard(),
            new RoundUpgradeChangeResult(),
            new UserOneWinRoundResultUpgradeWithBonusCard(),
            new UserOneWinWithPreviousUpgrades(),
            new UserOneWinRoundWithScorpionCardResult(),
            new CavalryWinWhenTeutonsFaith(),
            new WinWithPersianTC()
        };

        [TestCaseSource(nameof(_roundsCases))]
        public void GetCorrectWinner(IRoundResultTestCaseSource roundCase)
        {
            var pc = roundCase.PlayerCards;
            var roundUpgrade = roundCase.RoundUpgrade;
            var previousRounds = roundCase.PreviousRounds;
            
            var round = ARound(roundCase.Users, pc, roundUpgrade);
            var sm = ServerMatchMother.Create(MatchId, AUsers(roundCase.Users),
                BoardMother.Create(withRoundsPlayed: previousRounds, withCurrentRound: round));
            
            GivenUpgradeCards(pc, roundUpgrade, previousRounds);
            GivenMatchRepositoryReturns(sm);
            WhenExecute();
            ThenRoundWinnerIs();
            void ThenRoundWinnerIs()
            {
                Assert.AreEqual(roundCase.RoundWinners.Count,round.PlayerWinner.Count);
                Assert.IsTrue(roundCase.RoundWinners.All(rw=>round.PlayerWinner.Any(x=>x.Id == rw)));
            }
        }

        private void GivenUpgradeCards( Dictionary<string, PlayerCard> playerCards, UpgradeCard roundUpgrade, IList<Round> previousRounds)
        {
            var playerOneCards =
                new List<UpgradeCard>()
                {
                    roundUpgrade,
                    playerCards[UserIdOne].UpgradeCard
                };
            playerOneCards.AddRange(previousRounds.Select(round => round.PlayerCards[UserIdOne].UpgradeCard));

            _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserIdOne).Returns(playerOneCards);

            var playerTwoCards =
                new List<UpgradeCard>(previousRounds.Select(x => x.PlayerCards[UserIdTwo].UpgradeCard))
                {
                    roundUpgrade,
                    playerCards[UserIdTwo].UpgradeCard
                };
            playerTwoCards.AddRange(previousRounds.Select(round => round.PlayerCards[UserIdTwo].UpgradeCard));

            _getPlayerPlayedUpgradesInMatch.Execute(MatchId, UserIdTwo).Returns(playerTwoCards);
        }

        private void GivenMatchRepositoryReturns(ServerMatch sm) => _getMatch.Execute(MatchId).Returns(sm);
        private void WhenExecute() => _calculateRoundResultShould.Execute(MatchId);

        private IEnumerable<User> AUsers(IList<string> roundCaseUsers) =>
            roundCaseUsers.Select(player => UserMother.Create(player));
        private KeyValuePair<string, PlayerCard> APlayerOneInfo(PlayerCard withPlayerCard=null) =>
            new(UserIdOne,withPlayerCard ?? 
                          new PlayerCard()
                          {
                              UpgradeCard = UpgradeCardMother.Create(),
                              UnitCard = UnitCardMother.Create(),
                          });
        private KeyValuePair<string, PlayerCard> APlayerTwoInfo(PlayerCard withPlayerCard=null) =>
            new(UserIdTwo,withPlayerCard??
                          new PlayerCard()
                          {
                              UpgradeCard = UpgradeCardMother.Create(),
                              UnitCard = UnitCardMother.Create(),
                          });
        private Dictionary<string, PlayerCard> APlayerCards(KeyValuePair<string, PlayerCard> withPlayerOne, KeyValuePair<string, PlayerCard> withPlayerTwo) =>
            new()
            {
                {withPlayerOne.Key, withPlayerOne.Value},
                {withPlayerTwo.Key, withPlayerTwo.Value},
            };
        private  Round ARound(IList<string> withUsers, Dictionary<string, PlayerCard> withPlayerCards = null,UpgradeCard withRoundUpgradeCard = null )
        {
            return RoundMother.Create(withUsers,
                withPlayerCards ?? APlayerCards(APlayerOneInfo(), APlayerTwoInfo()),
                withRoundUpgradeCard: withRoundUpgradeCard ?? UpgradeCardMother.Create());
        }
    }
    
    #region TestCaseSources
    public interface IRoundResultTestCaseSource
    {
        IList<string> Users { get; }
        Dictionary<string, PlayerCard> PlayerCards { get; }
        IList<string> RoundWinners { get; }
        UpgradeCard RoundUpgrade { get; }
        IList<Round> PreviousRounds { get; }
    }
    
    public class EqualCardsTieRoundResult : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
            "Id-2"
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class CavalryWinWhenTeutonsFaith : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 0);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            //Heavy Scorpion,Siege,Siege Unit,0,Always wins vs Infantry,Infantry,99
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create("Cavalry", 1, new List<Archetype>
                    {Archetype.Cavalry}),
                UpgradeCard = UpgradeCardMother.CreateFakeTeutonsFaithCard()
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create("Monk", withBasePower: 0, withArchetypes:new List<Archetype>{ Archetype.Monk}, new List<Archetype>()
                    {
                        Archetype.Cavalry
                    }, 99 ),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    
    public class WinWithPersianTC : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 0);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            //Heavy Scorpion,Siege,Siege Unit,0,Always wins vs Infantry,Infantry,99
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create("Villager", 1, new List<Archetype>
                    {Archetype.Villager}),
                UpgradeCard = UpgradeCardMother.CreateFakePersianTC()
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create("Cavalry", withBasePower: 49, withArchetypes:new List<Archetype>{ Archetype.Cavalry} ),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>()
        {
            new Round(Users)
            {
                PlayerCards = new Dictionary<string, PlayerCard>
                {
                    {"Id", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:10,withCardName: "vill-upgrade",
                        withArchetypes:new List<Archetype> {Archetype.Villager})}},
                    {"Id-2", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:0)}}
                }
            },
            new Round(Users)
            {
                PlayerCards = new Dictionary<string, PlayerCard>
                {
                    {"Id", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:14,withCardName: "vill-upgrade",
                        withArchetypes:new List<Archetype> {Archetype.Villager})}},
                    {"Id-2", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:0)}}
                }
            }
        };
    }
    
    public class UserOneWinRoundWithScorpionCardResult : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 0);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            //Heavy Scorpion,Siege,Siege Unit,0,Always wins vs Infantry,Infantry,99
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create("Heavy Scorpion", 0, new List<Archetype>
                {Archetype.SiegeUnit}, new List<Archetype>{ Archetype.Infantry}, 99),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create("Pikeman", withBasePower: 4, withArchetypes:new List<Archetype>{ Archetype.Infantry}),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class UserOneWinRoundResult : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class UserTwoWinRoundResult : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id-2",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class UserOneWinRoundResultUpgradeCard : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:10)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:3)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class UserTwoWinRoundResultUpgradeCard : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id-2",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 4),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:10, withArchetypes: new List<Archetype>() {Archetype.Archer})
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:10)
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }
    public class UserOneWinRoundResultUpgradeWithBonusCard : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:15, withBonusVs: new List<Archetype>() {Archetype.Monk})
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:15, withBonusVs: new List<Archetype>() {Archetype.Archer})
            }}
        };
        public IList<Round> PreviousRounds => new List<Round>();
    }

    public class RoundUpgradeChangeResult : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5, withArchetypes: new List<Archetype>() {Archetype.Archer});

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 4, withArchetypes: new List<Archetype>() {Archetype.Archer}),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 5),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:0)
            }}
        };

        public IList<Round> PreviousRounds => new List<Round>();
    }

    public class UserOneWinWithPreviousUpgrades : IRoundResultTestCaseSource
    {
        public IList<string> Users => new List<string>
        {
            "Id",
            "Id-2"
        };

        public IList<string> RoundWinners => new List<string>
        {
            "Id",
        };

        public UpgradeCard RoundUpgrade => UpgradeCardMother.Create(withBasePower: 5);
        public IList<Round> PreviousRounds => new List<Round>()
        {
            new Round(Users)
            {
                PlayerCards = new Dictionary<string, PlayerCard>
                {
                    {"Id", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:10)}},
                    {"Id-2", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:0)}}
                }
            },
            new Round(Users)
            {
                PlayerCards = new Dictionary<string, PlayerCard>
                {
                    {"Id", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:50)}},
                    {"Id-2", new PlayerCard(){UpgradeCard = UpgradeCardMother.Create(withBasePower:0)}}
                }
            }
        };

        public Dictionary<string, PlayerCard> PlayerCards => new()
        {
            {"Id",new PlayerCard()
            {
                UnitCard = UnitCardMother.Create(withBasePower: 0),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:10)
            }},
            {"Id-2",new PlayerCard()
            {
                UnitCard =UnitCardMother.Create(withBasePower: 30),
                UpgradeCard = UpgradeCardMother.Create(withBasePower:10)
            }}
        };
    }
    #endregion
}