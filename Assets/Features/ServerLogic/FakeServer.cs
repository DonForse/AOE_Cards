using System;
using Features.ServerLogic;
using Features.ServerLogic.Matches.Action;
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
            _matchManager = new MatchManager(ServerLogicProvider.MatchesRepository(), new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
            _matchManager.Initialize();
        }

        private void OnDisable()
        {
            _matchManager.Dispose();
        }
    }
}