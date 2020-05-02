using System;
using System.Collections;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        void StartMatch(string playerId, Action<Match> onStartMatchComplete, Action<long,string> onError);
        void GetMatch(string playerId, Action<Match> onStartMatchComplete, Action<long, string> onError);
    }
}