using System;
using Features.ServerLogic;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Service;
using ServerLogic.Matches.Infrastructure;
using UnityEngine;

namespace ServerLogic
{
    public class FakeServer : MonoBehaviour
    {
        private MatchManager _matchManager;

        private void OnEnable()
        {
            _matchManager = new MatchManager(ServerLogicProvider.MatchesRepository(), new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
            _matchManager.Initialize();
        }

        private void OnDisable()
        {
            _matchManager.Dispose();
        }
    }
}