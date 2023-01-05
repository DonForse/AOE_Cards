using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Cards.Actions;
using Features.ServerLogic.Cards.Domain.Units;
using Features.ServerLogic.Cards.Domain.Upgrades;
using Features.ServerLogic.Cards.Infrastructure;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Matches.Service;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Action
{
    public class CreateMatch : ICreateMatch
    {
        private readonly IMatchesRepository _matchRepository;
        private readonly ICardRepository _cardRepository;
        private readonly ICreateBotUser _createBot;
        private readonly GetUnitCard _getUnitCard;
        private readonly GetUpgradeCard _getUpgradeCard;
        private readonly IServerConfiguration _serverConfiguration;
        private readonly IUserMatchesRepository _userMatchesRepository;

        public CreateMatch(IMatchesRepository matchRepository,
            ICardRepository cardRepository,
            IServerConfiguration serverConfiguration,
            ICreateBotUser createBotUser,
            IUserMatchesRepository userMatchesRepository)
        {
            _matchRepository = matchRepository;
            _cardRepository = cardRepository;
            _serverConfiguration = serverConfiguration;
            _createBot = createBotUser;
            _userMatchesRepository = userMatchesRepository;
            _getUnitCard = new GetUnitCard(_cardRepository);
            _getUpgradeCard = new GetUpgradeCard(_cardRepository);
        }

        public void Execute(IList<User> users, bool isBot, int botDifficulty = 0)
        {
            var match = CreateMatchInstance(users, isBot);
            match.BotDifficulty = botDifficulty;
            PersistMatch(match);
            PersistUsersToMatch(users, match);
        }

        private void PersistMatch(ServerMatch match) => _matchRepository.Add(match);

        private void PersistUsersToMatch(IEnumerable<User> users, ServerMatch match)
        {
            foreach (var user in users)
            {
                if (user.Id != "BOT")
                    _userMatchesRepository.Add(user.Id, match.Guid);
            }
        }

        private Domain.ServerMatch CreateMatchInstance(IList<User> users, bool isBotMatch)
        {
            if (isBotMatch)
                users.Add(_createBot.Execute());
            if (users.Count > 2)
                throw new ApplicationException("Match only allowed of two users");
            
            var match = new Domain.ServerMatch
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
                    UnitsCards = match.Board.Deck.TakeUnitCards(_serverConfiguration.GetAmountUnitCardsForPlayers()),
                    UpgradeCards = match.Board.Deck.TakeUpgradeCards(_serverConfiguration.GetAmountUpgradeCardForPlayers())
                });

                match.Board.PlayersHands[user.Id].UnitsCards.Add(_cardRepository.GetUnitCard("villager"));
            };
            var round = new Round(match.Users.Select(u => u.Id))
            {
                RoundUpgradeCard = match.Board.Deck.TakeUpgradeCards(1).FirstOrDefault(),
                roundNumber = 1,
            };
            round.ChangeRoundState(RoundState.Reroll);
            match.Board.CurrentRound = round;

            return match;
        }
    }
}