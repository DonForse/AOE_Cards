using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public partial class LoginService : MonoBehaviour, ILoginService
    {
        
        private string ApiUrl => Configuration.UrlBase + "/api/user/";
        private string PlayTurnUrl => Configuration.UrlBase + "/games/users/{0}/matches/{1}/play/{2}";

        public void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password });
            StartCoroutine(Put(data, onRegisterComplete, onRegisterFailed));
        }

        public void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password });
            StartCoroutine(Post(data, onLoginComplete, onLoginFailed));
        }

        private IEnumerator Put(string data, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            ResponseInfo response;
            Debug.Log("Put: " + ApiUrl);
            using (var webRequest = UnityWebRequest.Put(ApiUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPUT;
                webRequest.SetRequestHeader("Content-Type", "text/json");
                yield return webRequest.SendWebRequest();
                response = new ResponseInfo(webRequest);
                Debug.Log(response.response);
            }

            if (!response.isError && response.isComplete)
            {
                onLoginComplete(UserResponseDto.Parse(response.response));
            }
            else if (response.isError)
            {
                onLoginFailed(response.response);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Put(data, onLoginComplete, onLoginFailed));
            }
        }

        private IEnumerator Post(string data, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            ResponseInfo response;
            Debug.Log("Post: " + ApiUrl);
            using (var webRequest = UnityWebRequest.Post(ApiUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "text/json");
                yield return webRequest.SendWebRequest();
                response = new ResponseInfo(webRequest);
                Debug.Log(response.response);
            }

            if (!response.isError && response.isComplete)
            {
                onRegisterComplete(UserResponseDto.Parse(response.response));
            }
            else if (response.isError)
            {
                onRegisterFailed(response.response);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Post(data, onRegisterComplete, onRegisterFailed));
            }
        }
    }
}