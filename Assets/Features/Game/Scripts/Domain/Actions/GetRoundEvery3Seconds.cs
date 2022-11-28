using System;
using Features.Game.Scripts.Presentation;
using Infrastructure.Data;
using Infrastructure.Services;
using UniRx;

namespace Game
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
                Observable.Interval(TimeSpan.FromSeconds(3))
                    .Subscribe(_ =>
                    {
                        _playService.GetRound(_currentMatchRepository.Get().Board.Rounds.Count -1)
                            // .DoOnError(err => HandleError((PlayServiceException)err))
                            .Subscribe(emitter.OnNext)
                            .AddTo(_disposables);
                    }).AddTo(_disposables);
                return Disposable.Create(_disposables.Clear);
            });
        }
    }
}