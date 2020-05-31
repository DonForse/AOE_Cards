using System;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(bool vsBot, bool vsFriend, string friendCode, Action<Match> onStartMatchComplete, Action<long, string> onError);
        void GetMatch(Action<Match> onStartMatchComplete, Action<long, string> onError);
        void RemoveMatch(Action onRemoveMatchComplete, Action<long, string> onError);
    }
}