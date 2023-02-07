﻿using System.Linq;
using Features.ServerLogic.Game.Domain.Entities;
using Features.ServerLogic.Matches.Action;

namespace Features.ServerLogic.Bot.Domain.Entities
{
    public class Bot
    {
        private readonly IPlayUpgradeCard _playUpgradeCard;
        private readonly IPlayUnitCard _playUnitCard;
        public Bot(IPlayUpgradeCard playUpgradeCard, IPlayUnitCard playUnitCard)
        {
            _playUpgradeCard = playUpgradeCard;
            _playUnitCard = playUnitCard;
        }

        public void PlayCard(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            var round = serverMatch.Board.CurrentRound;
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
            if (serverMatch.Board.CurrentRound.PlayerCards["BOT"].UnitCard != null)
                return;
            _playUnitCard.Execute(serverMatch.Guid, "BOT", serverMatch.Board.PlayersHands["BOT"].UnitsCards.FirstOrDefault()?.cardName);
        }

        internal virtual void PlayUpgrade(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.CurrentRound.PlayerCards["BOT"].UpgradeCard != null)
                return;
            _playUpgradeCard.Execute(serverMatch.Guid, "BOT",
                serverMatch.Board.PlayersHands["BOT"].UpgradeCards.FirstOrDefault()?.cardName);
        }

        internal virtual void PlayReroll(Features.ServerLogic.Matches.Domain.ServerMatch serverMatch)
        {
            if (serverMatch.Board.CurrentRound.PlayerHasRerolled["BOT"])
                return;
            serverMatch.Board.CurrentRound.PlayerHasRerolled["BOT"] = true;
        }
    }
}
