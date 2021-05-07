using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class TokenService : MonoBehaviour, ITokenService
    {
        private string TokenUrl => Configuration.UrlBase + "/api/token";

        public IObservable<UserResponseDto> RefreshToken()
        {
            return Get().Retry(3);
        }
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