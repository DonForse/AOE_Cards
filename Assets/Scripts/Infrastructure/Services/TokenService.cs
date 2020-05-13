using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class TokenService : MonoBehaviour, ITokenService
    {
        private string TokenUrl => Configuration.UrlBase + "/api/token";

        public void RefreshToken(Action<UserResponseDto> onRefreshTokenComplete, Action<string> onError)
        {
            StartCoroutine(Get(onRefreshTokenComplete, onError));
        }
        private IEnumerator Get(Action<UserResponseDto> onPostComplete, Action<string> onPostFailed)
        {
            ResponseInfo responseInfo;
            Debug.Log(TokenUrl);
            using (var webRequest = UnityWebRequest.Get(TokenUrl))
            { 
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.RefreshToken));
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }
             if (responseInfo.isError)
            {
                onPostFailed(responseInfo.responseString);
            }
            else if (responseInfo.isComplete)
            {
                var dto = UserResponseDto.Parse(responseInfo.responseString);
                onPostComplete(dto);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(onPostComplete, onPostFailed));
            }
        }
    }
}