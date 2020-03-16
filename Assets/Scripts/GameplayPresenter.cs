using System;
using System.Collections.Generic;

public class GameplayPresenter
{
    private readonly GetDeck _getDeck;
    private Deck _deck;
    private readonly IList<IPlayer> _players;
    private readonly Dictionary<string, Hand> _playersHands;

    public GameplayPresenter(GetDeck getDeck, IList<IPlayer> players)
    {
        _getDeck = getDeck;
        _players = players;
        _playersHands = new Dictionary<string, Hand>();
    }

    public Hand GetHand(string playerId)
    {
        if (!_playersHands.ContainsKey(playerId)) {
            //_view.ShowError();
        }
        return _playersHands[playerId];
    }

    public void RoundSetup()
    {
        _deck = _getDeck.Execute();
        _deck.Shuffle();
        foreach (var player in _players)
        {
            _playersHands.Add(player.GetId(),
                new Hand()
                {
                    UnitCards = _deck.TakeUnitCards(5)
                });
        }

    }
}