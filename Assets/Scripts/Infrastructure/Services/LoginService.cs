using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class LoginService : MonoBehaviour, ILoginService
    {
        private const string BaseUrl = "https://localhost:44324/";
        private string ApiUrl => BaseUrl + "/api/user/";
        private string PlayTurnUrl => BaseUrl + "/games/users/{0}/matches/{1}/play/{2}";

        public void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password });
            StartCoroutine(Post(data, onRegisterComplete, onRegisterFailed));
        }

        public void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            var url = string.Format("{0}?username={1}&password={2}", ApiUrl, playerName, password);
            StartCoroutine(Get(url, onLoginComplete, onLoginFailed));
        }
        public class UserDto
        {
            public string username;
            public string password;
        }

        private IEnumerator Get(string url, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            bool isDone;
            bool isError;
            string responseString;
            using (var webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                isDone = webRequest.isDone;
                isError = webRequest.isError;
                responseString = isError ? webRequest.error : isDone ? webRequest.downloadHandler.text : string.Empty;
            }

            if (isDone)
            {
                onLoginComplete(UserResponseDto.Parse(responseString));
            }
            else if (isError)
            {
                onLoginFailed(responseString);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onLoginComplete, onLoginFailed));
            }
        }

        private IEnumerator Post(string data, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            bool isDone;
            bool isError;
            string responseString;
            using (var webRequest = UnityWebRequest.Post(ApiUrl, data))
            {
                webRequest.chunkedTransfer = false;
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "text/json");
                yield return webRequest.SendWebRequest();
                isDone = webRequest.isDone;
                isError = webRequest.isNetworkError;
                responseString = isError ? webRequest.error : isDone ? webRequest.downloadHandler.text : string.Empty;
            }

            if (isDone)
            {
                onRegisterComplete(UserResponseDto.Parse(responseString));
            }
            else if (isError)
            {
                onRegisterFailed(responseString);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Post(data, onRegisterComplete, onRegisterFailed));
            }
        }
    }
}