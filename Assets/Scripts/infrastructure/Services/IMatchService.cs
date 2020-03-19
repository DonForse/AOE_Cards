using System;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete);
        void PlayUpgradeCard(string cardName, Action<Round> onUpgradeCardsPlayed);
        void PlayUnitCard(string cardName, Action<RoundResult> onRoundFinished);
    }
}