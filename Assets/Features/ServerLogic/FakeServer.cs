using Features.ServerLogic.Matches.Service;
using UnityEngine;

namespace Features.ServerLogic
{
    public class FakeServer : MonoBehaviour
    {
        private PlayMatchService _playMatchService;

        private void OnEnable()
        {
            _playMatchService = new PlayMatchService(
                ServerLogicProvider.MatchesRepository(),
                ServerLogicProvider.PlayInactiveMatch(),
                ServerLogicProvider.RemoveUserMatch());
            _playMatchService.Initialize();
        }

        private void OnDisable()
        {
            _playMatchService.Dispose();
        }
    }
}