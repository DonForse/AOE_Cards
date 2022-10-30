using System;
using Data;
using Infrastructure.Data;

namespace Features.Game.Scripts.Domain
{
    public interface IGameView
    {
        public IObservable<string> PlayCard();
        void OnGetRoundInfo(Round round);
    }
}