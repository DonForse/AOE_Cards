public interface IMatchService
{
    MatchStatus StartMatch();
    void PlayEventCard(string cardName);
    void PlayUnitCard(string cardName);
}

public class MatchStatus
{
    public string round;
    public string hand;
    public string board;
}