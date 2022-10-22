using ServerLogic.Matches.Infrastructure;
using ServerLogic.Matches.Service;
using UnityEngine;

namespace ServerLogic
{
    public class FakeServer : MonoBehaviour
    {
        private MatchManager _matchManager;

        private void Start()
        {
            _matchManager = new MatchManager(ServerLogicProvider.MatchesRepository());
            _matchManager.Initialize();
        }
    }
}