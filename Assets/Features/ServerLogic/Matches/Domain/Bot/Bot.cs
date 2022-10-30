using System.Linq;

namespace ServerLogic.Matches.Domain.Bot
{
    public class Bot
    {
        public void PlayCard(Match match)
        {
            var round = match.Board.RoundsPlayed.Last();
            if (round.RoundState == RoundState.Reroll)
            {
                PlayReroll(match);
                return;
            }

            if (round.RoundState == RoundState.Upgrade)
            {
                PlayUpgrade(match);
                return;
            }

            if (round.RoundState == RoundState.Unit)
            {
                PlayUnit(match);
                return;
            }
        }

        internal virtual void PlayUnit(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerCards["BOT"].UnitCard != null)
                return;
            match.PlayUnitCard("BOT", match.Board.PlayersHands["BOT"].UnitsCards.FirstOrDefault());
        }

        internal virtual void PlayUpgrade(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerCards["BOT"].UpgradeCard != null)
                return;
            match.PlayUpgradeCard("BOT", match.Board.PlayersHands["BOT"].UpgradeCards.FirstOrDefault());
        }

        internal virtual void PlayReroll(Match match)
        {
            if (match.Board.RoundsPlayed.Last().PlayerReroll["BOT"])
                return;
            match.Board.RoundsPlayed.Last().PlayerReroll["BOT"] = true;
        }
    }
}

