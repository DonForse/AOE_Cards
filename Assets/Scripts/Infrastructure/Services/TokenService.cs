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
            ResponseInfo response;
            Debug.Log(TokenUrl);
            using (var webRequest = UnityWebRequest.Get(TokenUrl))
            { 
                webRequest.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString(PlayerPrefsHelper.RefreshToken));
                yield return webRequest.SendWebRequest();
                response = new ResponseInfo(webRequest);
                Debug.Log(response.response);
            }
             if (response.isError)
            {
                onPostFailed(response.response);
            }
            else if (response.isComplete)
            {
                var dto = UserResponseDto.Parse(response.response);
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