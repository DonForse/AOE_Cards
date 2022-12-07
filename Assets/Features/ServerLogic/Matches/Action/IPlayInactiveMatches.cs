using Features.ServerLogic.Matches.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public interface IPlayInactiveMatches
    {
        void Execute(ServerMatch serverMatch, Round round);
    }
}