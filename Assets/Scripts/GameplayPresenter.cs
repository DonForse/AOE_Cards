using System;
using System.Collections.Generic;
using System.Linq;

public class GameplayPresenter
{
    private readonly GetDeck _getDeck;
    private Deck _deck;
    private readonly IList<IPlayer> _players;
    private Hand _hand;
    private readonly IGameplayView _view;
    private readonly IMatchService _matchService;

    public GameplayPresenter(IGameplayView view, IMatchService matchService ,GetDeck getDeck, IList<IPlayer> players)
    {
        _view = view;
        _getDeck = getDeck;
        _players = players;
        _matchService = matchService;
    }

    public Hand GetHand()
    {
        return _hand;
    }

    public void GameSetup()
    {
        _deck = _getDeck.Execute();
        _deck.Shuffle();
        _hand = new Hand()
        {
            UnitCards = _deck.TakeUnitCards(5),
            EventCards = _deck.TakeEventCards(5),
        };

    }

    public void RoundSetup()
    {
        var card = _deck.TakeEventCards(1).FirstOrDefault();
        
        _view.ShowRoundEventCard(card);
    }

    public void PlayEventCard(string cardName)
    {
        var card = _hand.TakeEventCard(cardName);
        _matchService.PlayEventCard(card.cardName);
        _view.EventCardSentPlay();
    }

    public void PlayUnitCard(string cardName)
    {
        var card = _hand.TakeUnitCard(cardName);
        _matchService.PlayUnitCard(card.cardName);
        _view.UnitCardSentPlay();

    }
}