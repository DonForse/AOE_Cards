using Features.Infrastructure;
using Features.Infrastructure.Services;
using Features.Login;
using Features.Login.Scripts.Domain;
using Features.Match;
using Features.Match.Domain;
using Features.Token;
using Features.Token.Scripts.Domain;
using UnityEngine;

namespace Features
{
    public class ServicesProvider : MonoBehaviour
    {
        [SerializeField] private InMemoryCardProvider cardProvider;

        public IMatchService GetMatchService() => MatchProvider.OfflineMatchService(cardProvider);

        internal ILoginService GetLoginService() => LoginProvider.OfflineLoginService();

        internal ITokenService GetTokenService() => TokenProvider.OfflineTokenGateway();

        internal IPlayService GetPlayService() => PlayProvider.OfflinePlayService(cardProvider);
    }
}