using System;
using Infrastructure.Data;

namespace Features.Game.Scripts.Domain
{
    public interface IGetRoundEvery3Seconds
    {
        IObservable<Round> Execute();
    }
}