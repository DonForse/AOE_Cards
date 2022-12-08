using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
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
        private IMatchesRepository _matchesRepository;

        [SetUp]
        public void Setup()
        {
            _matchesRepository = Substitute.For<IMatchesRepository>();
            _calculateRoundResultShould = new CalculateRoundResult(_matchesRepository);
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
            new UserOneWinWithPreviousUpgrades()
        };
        
        [TestCaseSource(nameof(_roundsCases))]
        public void GetCorrectWinner(IRoundResultTestCaseSource roundCase)
        {
            var round = ARound(roundCase.Users, roundCase.PlayerCards, roundCase.RoundUpgrade);
            ServerMatch sm = new ServerMatch(){
                Users = AUsers(roundCase.Users), 
                Board = new Board
                {
                    RoundsPlayed = roundCase.PreviousRounds,
                    CurrentRound = round
                }};
            GivenMatchRepositoryReturns(sm);
            WhenExecute();
            ThenRoundWinnerIs();
            void ThenRoundWinnerIs()
            {
                Assert.AreEqual(roundCase.RoundWinners.Count,round.PlayerWinner.Count);
                Assert.IsTrue(roundCase.RoundWinners.All(rw=>round.PlayerWinner.Any(x=>x.Id == rw)));
            }
        }
        private void GivenMatchRepositoryReturns(ServerMatch sm) => _matchesRepository.Get(MatchId).Returns(sm);
        private void WhenExecute() => _calculateRoundResultShould.Execute(MatchId);
        private IList<User> AUsers(IList<string> roundCaseUsers) => roundCaseUsers.Select(player => new User() {Id = player}).ToList();
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
        private  Round ARound(IList<string> withUsers, Dictionary<string, PlayerCard> withPlayerCards = null,UpgradeCard withRoundUpgradeCard = null )
        {
            return new Round(withUsers)
            {
                PlayerCards = withPlayerCards ?? APlayerCards (APlayerOneInfo(), APlayerTwoInfo()),
                RoundUpgradeCard = withRoundUpgradeCard ?? new UpgradeCard(),
            };
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