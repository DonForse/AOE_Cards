using System;
using Infrastructure.DTOs;
using Infrastructure.Services;
using UniRx;

namespace Token
{
    public class OfflineTokenService : ITokenService
    {
        public IObservable<UserResponseDto> RefreshToken()
        {
            return Observable.Return(new UserResponseDto()
            {
                guid = PlayerPrefsHelper.UserId,
                username = PlayerPrefsHelper.UserName,
                refreshToken = PlayerPrefsHelper.RefreshToken,
                accessToken =  PlayerPrefsHelper.AccessToken,
                friendCode =  PlayerPrefsHelper.FriendCode
            });
        }
    }
}