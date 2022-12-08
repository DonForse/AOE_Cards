using Features.Token.Scripts.Domain;
using Features.Token.Scripts.Infrastructure;

namespace Features.Token
{
    public static class TokenProvider
    {
        private static ITokenService _tokenService;
        public static ITokenService TokenGateway() => _tokenService ??= new TokenGateway();

        public static ITokenService OfflineTokenGateway() => _tokenService??= new OfflineTokenService();
    }
}