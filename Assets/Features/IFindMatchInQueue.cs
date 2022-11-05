using System;
using System.Collections.Generic;
using Features.Match.Domain;

namespace Home
{
    public interface IFindMatchInQueue
    {
        IObservable<GameMatch> Execute();
    }
}