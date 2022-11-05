using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ServerLogic.Cards.Actions;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Cards.Infrastructure;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Matches.Service;
using ServerLogic.Users.Actions;
using ServerLogic.Users.Domain;

namespace ServerLogic.Matches.Action
{
    public class CreateMatch
    {
        private readonly IMatchesRepository _matchRepository;
        private readonly ICardRepository _cardRepository;
        private readonly CreateBotUser _createBot;
        private readonly GetUnitCard _getUnitCard;
        private readonly GetUpgradeCard _getUpgradeCard;

        public CreateMatch(IMatchesRepository matchRepository,
            ICardRepository cardRepository)
        {
            _matchRepository = matchRepository;
            _cardRepository = cardRepository;
            _createBot = new CreateBotUser();
            _getUnitCard = new GetUnitCard(_cardRepository);
            _getUpgradeCard = new GetUpgradeCard(_cardRepository);
        }

        public void Execute(IList<User> users, bool isBot, int botDifficulty = 0)
        {
            var match = CreateMatchInstance(users, isBot);
            match.BotDifficulty = botDifficulty;
            _matchRepository.Add(match);
        }
        private Features.ServerLogic.Matches.Domain.ServerMatch CreateMatchInstance(IList<User> users, bool isBotMatch)
        {
            if (isBotMatch)
                users.Add(_createBot.Execute()); 
            
            var match = new Features.ServerLogic.Matches.Domain.ServerMatch
            {
                Guid = Guid.NewGuid().ToString(),
                Board = new Board()
                {
                    RoundsPlayed = new List<Round>(),
                    PlayersHands = new Dictionary<string, Hand>(),
                    Deck = new Deck()
                },
                Users = users,
                IsBot = isBotMatch
            };
            match.Board.Deck.UnitCards = new ConcurrentStack<UnitCard>(_getUnitCard.Execute(false));
            match.Board.Deck.UpgradeCards = new ConcurrentStack<UpgradeCard>(_getUpgradeCard.Execute());
            match.Board.Deck.Shuffle();
            foreach (var user in users)
            {
                match.Board.PlayersHands.Add(user.Id, new Hand
                {
                    UnitsCards = match.Board.Deck.TakeUnitCards(ServerConfiguration.GetAmountUnitCardsForPlayers()),
                    UpgradeCards = match.Board.Deck.TakeUpgradeCards(ServerConfiguration.GetAmountUpgradeCardForPlayers())
                });

                match.Board.PlayersHands[user.Id].UnitsCards.Add(_cardRepository.GetUnitCard("villager"));
            };
            var round = new Round(match.Users.Select(u => u.Id))
            {
                RoundUpgradeCard = match.Board.Deck.TakeUpgradeCards(1).FirstOrDefault(),
                roundNumber = 1,
            };
            round.ChangeRoundState(RoundState.Reroll);
            match.Board.RoundsPlayed.Add(round);

            return match;
        }
    }
}