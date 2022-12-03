using System;
using System.Collections.Generic;
using Features.ServerLogic.Matches.Service;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Users.Domain;

namespace ServerLogic.Matches.Domain
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
        public IDictionary<string, bool> PlayerReroll;

        public int NextBotActionTimeInSeconds;
        public int Timer { get { return (int)(RoundPlayStartTime - DateTime.Now).TotalSeconds + new ServerConfiguration().GetRoundTimerDurationInSeconds(); } }

        public Round(IEnumerable<string> users)
        {
            PlayerReroll = new Dictionary<string, bool>();
            PlayerCards = new Dictionary<string, PlayerCard>();
            foreach (var user in users) {
                PlayerCards.Add(user, new PlayerCard());
                PlayerReroll.Add(user, false);
            }
            RoundPlayStartTime = DateTime.Now;
            NextBotActionTimeInSeconds = new Random().Next(new ServerConfiguration().GetMaxBotWaitForPlayRoundTimeInSeconds(), new ServerConfiguration().GetRoundTimerDurationInSeconds());
        }

        public void ChangeRoundState(RoundState roundState) {
            RoundState = roundState;
            RoundPlayStartTime = DateTime.Now;
        }
    }

    public enum RoundState
    {
        Reroll,
        Upgrade,
        Unit, 
        Finished,
        GameFinished,
    }
}

