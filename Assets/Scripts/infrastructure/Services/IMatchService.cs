using System;
using System.Collections;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        IEnumerator StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete);
        void PlayUpgradeCard(string cardName, Action<Round> onUpgradeCardsPlayed);
        void PlayUnitCard(string cardName, Action<RoundResult> onRoundFinished);
    }
}