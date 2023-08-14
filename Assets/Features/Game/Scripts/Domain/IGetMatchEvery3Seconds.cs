using System;
using Features.Match.Domain;

namespace Features.Game.Scripts.Domain
{
    public interface IGetMatchEvery3Seconds
    {
        IObservable<GameMatch> Execute();
    }
}