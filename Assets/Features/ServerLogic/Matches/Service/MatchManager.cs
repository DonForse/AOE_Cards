using System;
using System.Linq;
using System.Threading;
using Features.ServerLogic;
using Features.ServerLogic.Matches.Action;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Domain.Bot;
using ServerLogic.Matches.Infrastructure;

namespace ServerLogic.Matches.Service
{
    public class MatchManager : IDisposable
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IPlayUpgradeCard _playUpgradeCard;

        public MatchManager(IMatchesRepository matchesRepository, IPlayUpgradeCard playUpgradeCard)
        {
            _matchesRepository = matchesRepository;
            _playUpgradeCard = playUpgradeCard;
        }

        private static Timer Timer;
        private static HardBot hardBot;
        private static Bot easyBot;

        public void Initialize()
        {
            Timer = new Timer(PlayMatches, null, 3000, 3000);
            hardBot = new HardBot(new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
            easyBot = new Bot(new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }

        private void PlayMatches(object state)
        {
            Timer.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                var matchService = _matchesRepository;
                if (matchService == null)
                    return;
                PlayBotMatches(matchService);
                PlayInactiveMatches(matchService);
            }
            finally
            {
                Timer.Change(3000, Timeout.Infinite);
            }

        }

        private void PlayInactiveMatches(IMatchesRepository matchesRepository)
        {
            var matches = matchesRepository.GetAll();
            foreach (var match in matches)
            {
                var round = match.Board.RoundsPlayed.LastOrDefault();
                if (match.IsFinished) {
                    if (round.Timer < -300) //5 minutes 
                    {
                        foreach (var user in match.Users)
                        {
                            matchesRepository.RemoveByUserId(user.Id);
                        }
                    }
                    continue;
                }
                    
                
                if (IsLastRoundFinished(round))
                    continue;
                if (0 > round.Timer)
                {
                    try
                    {
                        PlayInactiveMatch(match, round);
                    }
                    catch { }
                }

            }
        }

        private void PlayInactiveMatch(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, Round round)
        {
            foreach (var pc in round.PlayerCards)
            {
                try
                {
                    if (round.RoundState == RoundState.Unit)
                    {
                        if (pc.Value.UnitCard == null)
                        {
                            serverMatch.PlayUnitCard(pc.Key, serverMatch.Board.PlayersHands[pc.Key].UnitsCards.First());
                            break;
                        }
                    }
                    if (round.RoundState == RoundState.Upgrade)
                    {
                        if (pc.Value.UpgradeCard == null)
                        {
                            _playUpgradeCard.Execute(serverMatch.Guid,pc.Key, serverMatch.Board.PlayersHands[pc.Key].UpgradeCards.First().CardName);
                            break;
                        }
                    }
                    if (round.RoundState == RoundState.Reroll)
                    {
                        round.PlayerReroll[pc.Key] = true;
                        if (round.PlayerReroll.Values.All(rerolled => rerolled))
                        {
                            round.ChangeRoundState(RoundState.Upgrade);
                            break;
                        }
                    }
                }

                catch { }
            }
        }

        private void PlayBotMatches(IMatchesRepository matchService)
        {
            var botMatches = matchService.GetAll().Where(m => m.IsBot);
            foreach (var match in botMatches)
            {
                if (match.IsFinished)
                {
                    OnBotMatchCompletion(matchService);
                    continue;
                }

                var round = match.Board.RoundsPlayed.LastOrDefault();
                if (IsLastRoundFinished(round))
                    continue;

                if (round.NextAction > round.Timer)
                {
                    try
                    {
                        PlayBotCards(match, round);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void PlayBotCards(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch, Round round)
        {
            if (serverMatch.BotDifficulty == 0)
            {
                easyBot.PlayCard(serverMatch);
                return;
            }
            hardBot.PlayCard(serverMatch);
        }

        private bool IsLastRoundFinished(Round round)
        {
            return round == null || round.RoundState == RoundState.Finished;
        }

        private void OnBotMatchCompletion(IMatchesRepository matchService)
        {
            matchService.RemoveByUserId("BOT");
        }
    }
}