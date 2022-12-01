using System;
using Features.ServerLogic;
using ServerLogic.Matches.Infrastructure;
using ServerLogic.Matches.Service;
using UnityEngine;

namespace ServerLogic
{
    public class FakeServer : MonoBehaviour
    {
        private MatchManager _matchManager;

        private void OnEnable()
        {
            _matchManager = new MatchManager(ServerLogicProvider.MatchesRepository());
            _matchManager.Initialize();
        }

        private void OnDisable()
        {
            _matchManager.Dispose();
        }
    }
}