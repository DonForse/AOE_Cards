using System;
using Features.Infrastructure.DTOs;
using Features.Infrastructure.Services;
using Features.Token.Scripts.Domain;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Features.Token.Scripts.Infrastructure
{
    public class TokenGateway : ITokenService
    {
        private string TokenUrl => Configuration.UrlBase + "/api/token";

        public IObservable<UserResponseDto> RefreshToken() => Get().Retry(3);

        private IObservable<UserResponseDto> Get()
        {
            return Observable.Create<UserResponseDto>(emitter =>
            {
                ResponseInfo responseInfo;
                Debug.Log(TokenUrl);
                var webRequest = UnityWebRequest.Get(TokenUrl);
                {
                    webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.RefreshToken));
                    return webRequest.SendWebRequest().AsObservable()
                    .DoOnCompleted(() => webRequest.Dispose())
                    .Subscribe(_ =>
                    {
                        responseInfo = new ResponseInfo(webRequest);
                        if (responseInfo.isError)
                        {
                            emitter.OnError(new Exception(responseInfo.response.error));
                        }
                        else if (responseInfo.isComplete)
                        {
                            var dto = UserResponseDto.Parse(responseInfo.response.response);
                            emitter.OnNext(dto);
                            emitter.OnCompleted();
                        }
                        else
                        {
                            throw new TimeoutException("Request Timed Out");
                        }
                    });
                }
            });
        }
    }
}