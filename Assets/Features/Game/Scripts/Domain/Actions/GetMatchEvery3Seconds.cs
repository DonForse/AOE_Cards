using System;
using Features.Match.Domain;
using UniRx;

namespace Features.Game.Scripts.Domain.Actions
{
    public class GetMatchEvery3Seconds : IGetMatchEvery3Seconds
    {
        private IMatchService _playService;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public GetMatchEvery3Seconds(IMatchService playService)
        {
            _playService = playService;
        }

        public IObservable<GameMatch> Execute()
        {
            return Observable.Create<GameMatch>(emitter =>
            {
                Observable.Interval(TimeSpan.FromMilliseconds(3000))
                    .Subscribe(_ =>
                    {
                        _playService.GetMatch()
                            .Subscribe(emitter.OnNext)
                            .AddTo(_disposables);
                    }).AddTo(_disposables);
                return Disposable.Create(_disposables.Clear);
            });
        }
    }
    public interface IGetMatchEvery3Seconds
    {
        IObservable<GameMatch> Execute();
    }
}