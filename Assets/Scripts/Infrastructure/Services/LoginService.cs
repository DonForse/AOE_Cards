using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class LoginService : MonoBehaviour, ILoginService
    {
        
        private string ApiUrl => Configuration.UrlBase + "/api/user/";
        private string PlayTurnUrl => Configuration.UrlBase + "/games/users/{0}/matches/{1}/play/{2}";

        public void Register(string playerName, string password, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            var dt = DateTime.Now;
            password = EncodePassword(password, dt);
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password, date = dt.ToString("dd-MM-yyyy hhmmss") });
            StartCoroutine(Put(data, onRegisterComplete, onRegisterFailed));
        }

        public void Login(string playerName, string password, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            var dt = DateTime.Now;
            password = EncodePassword(password, dt);
            string data = JsonUtility.ToJson(new UserDto { username = playerName, password = password, date = dt.ToString("dd-MM-yyyy hhmmss") });
            StartCoroutine(Post(data, onLoginComplete, onLoginFailed));
        }

        private string EncodePassword(string password, DateTime dt)
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");

            var phrase = "!AoE.MAG1#C-4nt11C4it##";
            byte[] bytesPhrase = encoding.GetBytes(phrase);

            var date = encoding.GetBytes(dt.ToString("dd-MM-yyyy hhmmss"));

            var bytePassword = encoding.GetBytes(password);
            var newPassword = new byte[bytePassword.Length];
            for (int i = 0; i < bytePassword.Length; i++)
            {
                var dateIndex = i;
                while (date.Length - dateIndex <= 0)
                {
                    dateIndex = dateIndex - date.Length;
                }
                var phraseIndex = i;
                while (bytesPhrase.Length - phraseIndex <= 0)
                {
                    phraseIndex = phraseIndex - bytesPhrase.Length;
                }
                newPassword[i] = (byte)((int)bytePassword[i] + (int)date[dateIndex] + (int)bytesPhrase[phraseIndex]);
            }
            return Convert.ToBase64String(newPassword);
        }


        private IEnumerator Put(string data, Action<UserResponseDto> onLoginComplete, Action<string> onLoginFailed)
        {
            ResponseInfo responseInfo;
            Debug.Log("Put: " + ApiUrl);
            using (var webRequest = UnityWebRequest.Put(ApiUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPUT;
                webRequest.SetRequestHeader("Content-Type", "text/json;charset=ISO-8859-1");
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }

            if (!responseInfo.isError && responseInfo.isComplete)
            {
                onLoginComplete(UserResponseDto.Parse(responseInfo.responseString));
            }
            else if (responseInfo.isError)
            {
                onLoginFailed(responseInfo.responseString);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Put(data, onLoginComplete, onLoginFailed));
            }
        }

        private IEnumerator Post(string data, Action<UserResponseDto> onRegisterComplete, Action<string> onRegisterFailed)
        {
            ResponseInfo responseInfo;
            Debug.Log("Post: " + ApiUrl);
            using (var webRequest = UnityWebRequest.Post(ApiUrl, data))
            {
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "text/json;charset=ISO-8859-1");
                yield return webRequest.SendWebRequest();
                responseInfo = new ResponseInfo(webRequest);
            }

            if (!responseInfo.isError && responseInfo.isComplete)
            {
                onRegisterComplete(UserResponseDto.Parse(responseInfo.responseString));
            }
            else if (responseInfo.isError)
            {
                onRegisterFailed(responseInfo.responseString);
            }
            else
            {
                yield return new WaitForSeconds(3f);
                StartCoroutine(Post(data, onRegisterComplete, onRegisterFailed));
            }
        }
    }
}