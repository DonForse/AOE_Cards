namespace ServerLogic.Cards.Domain.Units
{
    public interface IUnitCard
    {
        int CalculatePower(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId);
        void Play(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, string userId);
    }
}