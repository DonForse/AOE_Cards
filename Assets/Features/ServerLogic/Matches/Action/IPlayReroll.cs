using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayReroll
    {
        void Execute(string matchId, string userId, RerollInfoDto cards);
    }
}