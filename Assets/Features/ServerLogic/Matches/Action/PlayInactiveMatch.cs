using System.Linq;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayInactiveMatch : IPlayInactiveMatch
    {
        private readonly IPlayUnitCard _playUnitCard;
        private readonly IPlayUpgradeCard _playUpgradeCard;

        public PlayInactiveMatch(IPlayUnitCard playUnitCard, IPlayUpgradeCard playUpgradeCard)
        {
            _playUnitCard = playUnitCard;
            _playUpgradeCard = playUpgradeCard;
        }

        public void Execute(ServerMatch serverMatch, Round round)
        {
            foreach (var pc in round.PlayerCards)
            {
                try
                {
                    if (round.RoundState == RoundState.Unit)
                    {
                        if (pc.Value.UnitCard == null)
                        {
                            _playUnitCard.Execute(serverMatch.Guid, pc.Key, serverMatch.Board.PlayersHands[pc.Key].UnitsCards.First().CardName);
                            continue;
                        }
                    }
                    if (round.RoundState == RoundState.Upgrade)
                    {
                        if (pc.Value.UpgradeCard == null)
                        {
                            _playUpgradeCard.Execute(serverMatch.Guid,pc.Key, serverMatch.Board.PlayersHands[pc.Key].UpgradeCards.First().CardName);
                            continue;
                        }
                    }
                    if (round.RoundState == RoundState.Reroll)
                    {
                        round.PlayerReroll[pc.Key] = true;
                        if (round.PlayerReroll.Values.All(rerolled => rerolled))
                        {
                            round.ChangeRoundState(RoundState.Upgrade);
                            break;
                        }
                    }
                }

                catch { }
            }
        }

    }
}