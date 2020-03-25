using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class LoginService : MonoBehaviour, ILoginService
    {
        private const string BaseUrl = "https://localhost:44324/";
        private string ApiUrl => BaseUrl + "/api/user/";
        private string PlayTurnUrl => BaseUrl + "/games/users/{0}/matches/{1}/play/{2}";

        public void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete)
        {
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password });
            StartCoroutine(Post(data, onRegisterComplete));
        }

        public void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete)
        {
            var url = string.Format("{0}?username={1}&password={2}", ApiUrl, playerName, password);
            StartCoroutine(Get(url, onLoginComplete));
        }
        public class UserDto
        {
            public string username;
            public string password;
        }

        private IEnumerator Get(string url, Action<UserResponseDto> onLoginComplete)
        {
            bool isDone;
            bool isError;
            string responseString;
            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                isDone = www.isDone;
                isError = www.isError;
                responseString = isDone ? www.downloadHandler.text : www.error;
            }

            if (isDone)
            {
                onLoginComplete(responseString);
            }
            else if (isError)
            {
                throw new ApplicationException("");
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Get(url, onLoginComplete));
            }
        }

        private IEnumerator Post(string data, Action<UserResponseDto> onRegisterComplete)
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
                responseString = isDone ? webRequest.downloadHandler.text : webRequest.error;
            }

            if (isDone)
            {
                //var dto = JsonUtility.FromJson<MatchStatusDto>(responseString)
                //onStartMatchComplete(DtoToMatchStatus(dto));
                onRegisterComplete(UserResponseDto.Parse(responseString));
            }
            else if (isError)
            {
                throw new ApplicationException("");
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Post(data, onRegisterComplete));
            }
        }
    }
}