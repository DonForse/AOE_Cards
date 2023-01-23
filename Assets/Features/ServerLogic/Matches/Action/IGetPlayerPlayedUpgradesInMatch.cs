using System.Collections.Generic;
using Features.ServerLogic.Cards.Domain.Entities;

namespace Features.ServerLogic.Matches.Action
{
    public interface IGetPlayerPlayedUpgradesInMatch
    {
        IEnumerable<UpgradeCard> Execute(string matchId, string userId);
    }
}