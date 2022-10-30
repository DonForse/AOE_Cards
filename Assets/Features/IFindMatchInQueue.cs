using System;
using System.Collections.Generic;

namespace Home
{
    public interface IFindMatchInQueue
    {
        IObservable<Match.Domain.Match> Execute();
    }
}