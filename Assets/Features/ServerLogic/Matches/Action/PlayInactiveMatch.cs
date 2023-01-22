using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace Features.ServerLogic.Matches.Action
{
    public class PlayInactiveMatch : IPlayInactiveMatch
    {
        private readonly IPlayUnitCard _playUnitCard;
        private readonly IPlayUpgradeCard _playUpgradeCard;
        private readonly IPlayReroll _playReroll;

        public PlayInactiveMatch(IPlayUnitCard playUnitCard, IPlayUpgradeCard playUpgradeCard, IPlayReroll playReroll)
        {
            _playUnitCard = playUnitCard;
            _playUpgradeCard = playUpgradeCard;
            _playReroll = playReroll;
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
                            _playUnitCard.Execute(serverMatch.Guid, pc.Key, serverMatch.Board.PlayersHands[pc.Key].UnitsCards.First().cardName);
                            continue;
                        }
                    }
                    if (round.RoundState == RoundState.Upgrade)
                    {
                        if (pc.Value.UpgradeCard == null)
                        {
                            _playUpgradeCard.Execute(serverMatch.Guid,pc.Key, serverMatch.Board.PlayersHands[pc.Key].UpgradeCards.First().cardName);
                            continue;
                        }
                    }
                    if (round.RoundState == RoundState.Reroll)
                    {
                        if (!round.PlayerHasRerolled[pc.Key])
                            _playReroll.Execute(serverMatch, pc.Key,
                                new RerollInfoDto()
                                    {unitCards = new List<string>(), upgradeCards = new List<string>()});
                    }
                }

                catch { }
            }
        }

    }
}