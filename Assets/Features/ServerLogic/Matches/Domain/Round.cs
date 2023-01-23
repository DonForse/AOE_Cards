using System;
using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Domain
{
    public class Round
    {
        public RoundState RoundState { get; private set; }
        public int roundNumber;
        public IList<User> PlayerWinner;
        public IDictionary<string, PlayerCard> PlayerCards;
        public UpgradeCard RoundUpgradeCard;
        public DateTime RoundPlayStartTime { get; private set; }

        //public bool HasReroll;
        public IDictionary<string, bool> PlayerHasRerolled;

        public int NextBotActionTimeInSeconds;
        public int Timer { get { return (int)(RoundPlayStartTime - DateTime.Now).TotalSeconds + new ServerConfiguration().GetRoundTimerDurationInSeconds(); } }

        public Round(IEnumerable<string> users)
        {
            PlayerHasRerolled = new Dictionary<string, bool>();
            PlayerCards = new Dictionary<string, PlayerCard>();
            foreach (var user in users) {
                PlayerCards.Add(user, new PlayerCard());
                PlayerHasRerolled.Add(user, false);
            }
            RoundPlayStartTime = DateTime.Now;
            NextBotActionTimeInSeconds = new Random().Next(new ServerConfiguration().GetMaxBotWaitForPlayRoundTimeInSeconds(), new ServerConfiguration().GetRoundTimerDurationInSeconds());
        }

        public void ChangeRoundState(RoundState roundState) {
            RoundState = roundState;
            RoundPlayStartTime = DateTime.Now;
        }
    }
}

