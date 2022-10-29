using System;
using Infrastructure.Services.Exceptions;
using Match.Domain;
using UniRx;

namespace Home
{
    public class FindMatchInQueue : IFindMatchInQueue
    {
        private readonly IMatchService _matchService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public FindMatchInQueue(IMatchService matchService)
        {
            _matchService = matchService;
        }

        public IObservable<Match.Domain.Match> Execute()
        {
            return Observable.Create<Match.Domain.Match>(emitter =>
            {
                Observable.Interval(TimeSpan.FromSeconds(3))
                    .Subscribe(_ =>
                    {
                        _matchService.GetMatch()
                            // .DoOnError(err => HandleError((MatchServiceException)err))
                            .Subscribe(match =>
                            {
                                if (match == null) return;
                                
                                emitter.OnNext(match);
                                emitter.OnCompleted();
                                _disposables.Clear();
                            }).AddTo(_disposables);
                    })
                    .AddTo(_disposables);
                return Disposable.Empty;
            });
            
        }
        
        private void HandleError(MatchServiceException exception)
        {
            if (exception.Code != 401)
            {
                // _view.OnError(exception.Error);
                // _onError.OnNext(exception.Error);
                return;
            }

            // _tokenService.RefreshToken()
            //     .DoOnError(err => OnRefreshTokenError(err.Message))
            //     .Subscribe(OnRefreshTokenComplete).AddTo(_disposables);
            // return;
        }

    }
}