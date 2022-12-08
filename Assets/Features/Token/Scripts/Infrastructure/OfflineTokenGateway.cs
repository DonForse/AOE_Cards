using System;
using Features.Infrastructure.DTOs;
using Features.Token.Scripts.Domain;
using UniRx;

namespace Features.Token.Scripts.Infrastructure
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