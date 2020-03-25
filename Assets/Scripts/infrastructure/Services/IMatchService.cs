using System;
using System.Collections;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete, Action<string> onError);
        void PlayUpgradeCard(string cardName, Action<Round> onUpgradeCardsPlayed,Action<string> onError);
        void PlayUnitCard(string cardName, Action<RoundResult> onRoundFinished,Action<string> onError);
    }
}