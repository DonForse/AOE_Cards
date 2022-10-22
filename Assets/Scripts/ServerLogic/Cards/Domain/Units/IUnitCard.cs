namespace ServerLogic.Cards.Domain.Units
{
    public interface IUnitCard
    {
        int CalculatePower(Matches.Domain.Match match, string userId);
        void Play(Matches.Domain.Match match, string userId);
    }
}