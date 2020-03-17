public interface IGameView
{
    void ShowRoundEventCard(EventCardData eventCard);
    void ShowHand();
    void ShowUnitCard();
    void ShowEventCard();
    void ShowPlayerEventsPlayed();
    void ShowError();
    void EventCardSentPlay();
    void UnitCardSentPlay();
}