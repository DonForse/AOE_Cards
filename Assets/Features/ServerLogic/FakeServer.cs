using System;
using Features.ServerLogic;
using Features.ServerLogic.Matches.Action;
using Features.ServerLogic.Matches.Service;
using UnityEngine;

namespace ServerLogic
{
    public class FakeServer : MonoBehaviour
    {
        private BotPlayService _botPlayService;

        private void OnEnable()
        {
            _botPlayService = new BotPlayService(ServerLogicProvider.MatchesRepository(), new PlayUpgradeCard(ServerLogicProvider.MatchesRepository(), ServerLogicProvider.CardRepository()));
            _botPlayService.Initialize();
        }

        private void OnDisable()
        {
            _botPlayService.Dispose();
        }
    }
}