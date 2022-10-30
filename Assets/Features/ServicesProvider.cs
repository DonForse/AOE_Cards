using Infrastructure;
using Infrastructure.Services;
using Login;
using Login.Scripts.Domain;
using Match;
using Match.Domain;
using Token;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    [SerializeField] private InMemoryCardProvider cardProvider;

    public IMatchService GetMatchService() => MatchProvider.OfflineMatchService(cardProvider);

    internal ILoginService GetLoginService() => LoginProvider.OfflineLoginService();

    internal ITokenService GetTokenService() => TokenProvider.OfflineTokenGateway();

    internal IPlayService GetPlayService() => PlayProvider.OfflinePlayService(cardProvider);
}