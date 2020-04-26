using System;

namespace Infrastructure.Services
{
    public  interface ITokenService
    {
        void RefreshToken(Action<UserResponseDto> onRefreshTokenComplete, Action<string> onError);
    }
}