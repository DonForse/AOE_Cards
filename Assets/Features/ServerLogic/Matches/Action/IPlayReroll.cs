using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure.DTO;

namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayReroll
    {
        void Execute(ServerMatch serverMatch, string userId, RerollInfoDto cards);
    }
}