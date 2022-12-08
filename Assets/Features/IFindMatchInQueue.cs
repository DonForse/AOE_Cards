using System;
using Features.Match.Domain;

namespace Features
{
    public interface IFindMatchInQueue
    {
        IObservable<GameMatch> Execute();
    }
}