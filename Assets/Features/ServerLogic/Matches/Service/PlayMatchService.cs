using System;
using System.Linq;
using System.Threading;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Domain;
using Features.ServerLogic.Matches.Domain.Bot;
using Features.ServerLogic.Matches.Infrastructure;
using Features.ServerLogic.Users.Actions;

namespace Features.ServerLogic.Matches.Service
{
    public class PlayMatchService : IDisposable
    {
        private readonly IMatchesRepository _matchesRepository;
        private readonly IPlayInactiveMatch _playInactiveMatch;
        private readonly IRemoveUserMatch _removeUserMatch;

        public PlayMatchService(IMatchesRepository matchesRepository, IPlayInactiveMatch playInactiveMatch)
        {
            _matchesRepository = matchesRepository;
            _playInactiveMatch = playInactiveMatch;
        }

        private static Timer Timer;
        private static HardBot hardBot;
        private static Bot easyBot;
        private IPlayUnitCard _playUnitCard;

        public void Initialize()
        {
            Timer = new Timer(PlayMatches, null, 3000, 3000);
            hardBot = new HardBot(new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()), new PlayUnitCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
            easyBot = new Bot(new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()), new PlayUnitCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
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
                ProcessInactiveMatches(matchService);
            }
            finally
            {
                Timer.Change(3000, Timeout.Infinite);
            }

        }

        private void ProcessInactiveMatches(IMatchesRepository matchesRepository)
        {
            var matches = matchesRepository.GetAll();
            foreach (var match in matches)
            {
                var round = match.Board.RoundsPlayed.LastOrDefault();
                if (match.IsFinished)
                {
                    RemoveFinishedMatchesAfterDelay(round, match);
                    continue;
                }
                
                if (IsLastRoundFinished(round))
                    continue;
                if (!RoundPhaseTimedOut(round)) continue;
                try
                {
                    _playInactiveMatch.Execute(match, round);
                }
                catch { }

            }
        }

        private static bool RoundPhaseTimedOut(Round round) => 0 > round.Timer;

        private void RemoveFinishedMatchesAfterDelay(Round round,
            ServerMatch match)
        {
            if (round.Timer < -300) //5 minutes 
            {
                foreach (var user in match.Users)
                {
                    _removeUserMatch.Execute(user.Id);
                }
            }
        }

        private void PlayBotMatches(IMatchesRepository matchService)
        {
            var botMatches = matchService.GetAll().Where(m => m.IsBot);
            foreach (var match in botMatches)
            {
                if (match.IsFinished)
                {
                    OnBotMatchCompletion();
                    continue;
                }

                var round = match.Board.RoundsPlayed.LastOrDefault();
                if (IsLastRoundFinished(round))
                    continue;

                if (round.NextBotActionTimeInSeconds > round.Timer)
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

        private void OnBotMatchCompletion()
        {
            _removeUserMatch.Execute("BOT");
        }
    }
}