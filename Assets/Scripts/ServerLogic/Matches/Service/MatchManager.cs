using System.Linq;
using System.Threading;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Domain.Bot;
using ServerLogic.Matches.Infrastructure;

namespace ServerLogic.Matches.Service
{
    public class MatchManager
    {
        private readonly IMatchesRepository _matchesRepository;

        public MatchManager(IMatchesRepository matchesRepository)
        {
            _matchesRepository = matchesRepository;
        }

        private static Timer Timer;
        private static HardBot hardBot;
        private static Bot easyBot;
        public void Initialize()
        {
            Timer = new Timer(PlayMatches, null, 3000, 3000);
            hardBot = new HardBot();
            easyBot = new Bot();
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

        private void PlayInactiveMatch(Domain.Match match, Round round)
        {
            foreach (var pc in round.PlayerCards)
            {
                try
                {
                    if (round.RoundState == RoundState.Unit)
                    {
                        if (pc.Value.UnitCard == null)
                        {
                            match.PlayUnitCard(pc.Key, match.Board.PlayersHands[pc.Key].UnitsCards.First());
                            break;
                        }
                    }
                    if (round.RoundState == RoundState.Upgrade)
                    {
                        if (pc.Value.UpgradeCard == null)
                        {
                            match.PlayUpgradeCard(pc.Key, match.Board.PlayersHands[pc.Key].UpgradeCards.First());
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

        private void PlayBotCards(Domain.Match match, Round round)
        {
            if (match.BotDifficulty == 0)
            {
                easyBot.PlayCard(match);
                return;
            }
            hardBot.PlayCard(match);
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