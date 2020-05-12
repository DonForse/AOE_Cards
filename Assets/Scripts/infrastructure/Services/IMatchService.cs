using System;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<long,string> onError);
        void GetMatch(string playerId, Action<Match> onStartMatchComplete, Action<long, string> onError);
        void RemoveMatch(Action onRemoveMatchComplete, Action<long, string> onError);
    }
}