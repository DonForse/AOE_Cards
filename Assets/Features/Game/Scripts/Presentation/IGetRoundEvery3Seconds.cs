using System;
using System.Collections.Generic;
using Infrastructure.Data;
using Infrastructure.Services.Exceptions;
using ServerLogic.Matches.Infrastructure;

namespace Game
{
    public interface IGetRoundEvery3Seconds
    {
        IObservable<Round> Execute();
    }
}