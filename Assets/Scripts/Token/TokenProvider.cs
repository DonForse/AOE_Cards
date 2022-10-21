namespace Infrastructure.Services
{
    public static class TokenProvider
    {
        private static ITokenService _tokenService;
        public static ITokenService TokenService() => _tokenService ??= new TokenService();
    }
}