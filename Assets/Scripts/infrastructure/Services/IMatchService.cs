using System;
using System.Collections;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<string> onError);
    }
}