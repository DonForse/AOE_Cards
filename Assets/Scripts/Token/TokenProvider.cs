namespace Token
{
    public static class TokenProvider
    {
        private static ITokenService _tokenService;
        public static ITokenService TokenService() => _tokenService ??= new TokenService();

        public static ITokenService OfflineTokenService() => _tokenService??= new OfflineTokenService();
    }
}