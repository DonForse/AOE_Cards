using System;

namespace Infrastructure.Services
{
    public class OfflineTokenService : ITokenService
    {
        public IObservable<UserResponseDto> RefreshToken()
        {
            throw new NotImplementedException();
        }
    }
}