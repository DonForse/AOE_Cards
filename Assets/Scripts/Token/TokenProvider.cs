using Token.Scripts.Infrastructure;

namespace Token
{
    public static class TokenProvider
    {
        private static ITokenService _tokenService;
        public static ITokenService TokenGateway() => _tokenService ??= new TokenGateway();

        public static ITokenService OfflineTokenGateway() => _tokenService??= new OfflineTokenService();
    }
}