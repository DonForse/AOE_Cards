using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayInactiveMatch
    {
        void Execute(ServerMatch serverMatch, Round round);
    }
}