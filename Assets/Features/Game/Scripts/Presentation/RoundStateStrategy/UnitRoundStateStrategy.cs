﻿using Features.Game.Scripts.Domain;
using Infrastructure.Data;

namespace Features.Game.Scripts.Presentation.RoundStateStrategy
{
    public class UnitRoundStateStrategy : IRoundStateStrategy
    {
        private readonly IMatchStateRepository _matchStateRepository;
        private readonly IGameView _gameView;

        public UnitRoundStateStrategy(IGameView gameView, IMatchStateRepository matchStateRepository)
        {
            _matchStateRepository = matchStateRepository;
            _gameView = gameView;
        }

        public bool IsValid(Round round) => round.RoundState == RoundState.Unit;

        public void Execute(Round round)
        {
            var matchState = _matchStateRepository.Get();
            if (matchState.IsUpgradePhase())
            {
                _matchStateRepository.Set(GameState.UpgradeReveal);
                //en callback de coroutina de la vista
                _gameView.ShowUpgradeCardsPlayedRound(round, () =>
                {
                    _matchStateRepository.Set(GameState.StartUnit);
                    // isWorking = false;
                });
                return;
            }

            if (round.RivalReady)
            {
                _gameView.ShowRivalWaitUnit();
            }
        }
    }
}