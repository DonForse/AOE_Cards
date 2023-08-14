﻿using System;
using Features.Infrastructure.Data;
using Features.Infrastructure.Services;
using UniRx;

namespace Features.Game.Scripts.Domain.Actions
{
    public class GetRoundEvery3Seconds : IGetRoundEvery3Seconds
    {
        private IPlayService _playService;
        private ICurrentMatchRepository _currentMatchRepository;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public GetRoundEvery3Seconds(IPlayService playService, ICurrentMatchRepository currentMatchRepository)
        {
            _playService = playService;
            _currentMatchRepository = currentMatchRepository;
        }

        public IObservable<Round> Execute()
        {
            return Observable.Create<Round>(emitter =>
            {
                Observable.Interval(TimeSpan.FromMilliseconds(3000))
                    .Subscribe(_ =>
                    {
                        _playService.GetRound(_currentMatchRepository.Get().Board.Rounds.Count -1)
                            .Subscribe(emitter.OnNext)
                            .AddTo(_disposables);
                    }).AddTo(_disposables);
                return Disposable.Create(_disposables.Clear);
            });
        }
        
        
    }
}