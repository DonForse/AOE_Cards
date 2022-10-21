using Infrastructure;
using Infrastructure.Services;
using Login;
using Login.Scripts.Domain;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    [SerializeField] private InMemoryCardProvider cardProvider;

    public IMatchService GetMatchService() => MatchProvider.MatchService(cardProvider);

    internal ILoginService GetLoginService() => LoginProvider.LoginService();

    internal ITokenService GetTokenService() => TokenProvider.TokenService();

    internal IPlayService GetPlayService() => PlayProvider.PlayService(cardProvider);
}