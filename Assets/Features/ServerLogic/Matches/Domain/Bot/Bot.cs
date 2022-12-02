﻿using System.Linq;
using Features.ServerLogic.Matches.Action;

namespace ServerLogic.Matches.Domain.Bot
{
    public class Bot
    {
        private readonly IPlayUpgradeCard _playUpgradeCard;

        public Bot(IPlayUpgradeCard playUpgradeCard)
        {
            _playUpgradeCard = playUpgradeCard;
        }

        public void PlayCard(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            var round = serverMatch.Board.RoundsPlayed.Last();
            if (round.RoundState == RoundState.Reroll)
            {
                PlayReroll(serverMatch);
                return;
            }

            if (round.RoundState == RoundState.Upgrade)
            {
                PlayUpgrade(serverMatch);
                return;
            }

            if (round.RoundState == RoundState.Unit)
            {
                PlayUnit(serverMatch);
                return;
            }
        }

        internal virtual void PlayUnit(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.RoundsPlayed.Last().PlayerCards["BOT"].UnitCard != null)
                return;
            serverMatch.PlayUnitCard("BOT", serverMatch.Board.PlayersHands["BOT"].UnitsCards.FirstOrDefault());
        }

        internal virtual void PlayUpgrade(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.RoundsPlayed.Last().PlayerCards["BOT"].UpgradeCard != null)
                return;
            _playUpgradeCard.Execute(serverMatch.Guid, "BOT",
                serverMatch.Board.PlayersHands["BOT"].UpgradeCards.FirstOrDefault()?.CardName);
        }

        internal virtual void PlayReroll(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.RoundsPlayed.Last().PlayerReroll["BOT"])
                return;
            serverMatch.Board.RoundsPlayed.Last().PlayerReroll["BOT"] = true;
        }
    }
}

