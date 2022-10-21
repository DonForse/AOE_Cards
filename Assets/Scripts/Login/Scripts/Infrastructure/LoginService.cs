using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Services;
using Login.Scripts.Domain;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Login.Scripts.Infrastructure
{
    public class LoginService : ILoginService
    {
        public LoginService()
        {
            _disposables = new CompositeDisposable();
        }

        private CompositeDisposable _disposables;
        private string ApiUrl => Configuration.UrlBase + "/api/user/";
        private string PlayTurnUrl => Configuration.UrlBase + "/games/users/{0}/matches/{1}/play/{2}";

        public IObservable<UserResponseDto> Register(string playerName, string password)
        {
            var dt = DateTime.Now;
            password = EncodePassword(password, dt);
            string data = JsonUtility.ToJson(new UserDto
            { username = playerName, password = password, date = dt.ToString("dd-MM-yyyy hhmmss") });
            return Put(data).Retry(3);
        }

        public IObservable<UserResponseDto> Login(string playerName, string password)
        {
            var dt = DateTime.Now;
            password = EncodePassword(password, dt);
            string data = JsonUtility.ToJson(new UserDto
            { username = playerName, password = password, date = dt.ToString("dd-MM-yyyy hhmmss") });
            return Post(data).Retry(3);
        }
        public void Unload()
        {
            _disposables.Clear();
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

                newPassword[i] =
                    (byte)((int)bytePassword[i] + (int)date[dateIndex] + (int)bytesPhrase[phraseIndex]);
            }

            return Convert.ToBase64String(newPassword);
        }


        private IObservable<UserResponseDto> Put(string data)
        {
            return Observable.Create<UserResponseDto>(emitter =>
            {
                ResponseInfo responseInfo;
                Debug.Log("Put: " + ApiUrl);
                var webRequest = UnityWebRequest.Put(ApiUrl, data);
                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPUT;
                webRequest.SetRequestHeader("Content-Type", "text/json;charset=ISO-8859-1");

                return webRequest.SendWebRequest().AsObservable()
                .ObserveOn(Scheduler.MainThread)
                .DoOnCompleted(() => webRequest.Dispose())
                .Subscribe(operation =>
                {
                    responseInfo = new ResponseInfo(webRequest);
                    if (!responseInfo.isError && responseInfo.isComplete)
                    {
                        emitter.OnNext(UserResponseDto.Parse(responseInfo.response.response));
                        emitter.OnCompleted();
                    }
                    else if (responseInfo.isError)
                    {
                        emitter.OnError(new Exception(responseInfo.response.error));
                        emitter.OnCompleted();
                    }
                    else
                    {
                        throw new TimeoutException("server response not received");
                    }
                });
            });
        }
        private IObservable<UserResponseDto> Post(string data)
        {
            return Observable.Create<UserResponseDto>(emitter =>
            {
                ResponseInfo responseInfo;
                Debug.Log("Post: " + ApiUrl);
                var webRequest = UnityWebRequest.Post(ApiUrl, data);

                byte[] jsonToSend = Encoding.UTF8.GetBytes(data);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.method = UnityWebRequest.kHttpVerbPOST;
                webRequest.SetRequestHeader("Content-Type", "text/json;charset=ISO-8859-1");

                return webRequest.SendWebRequest()
                .AsObservable()
                    .DoOnCompleted(() => webRequest.Dispose())
                    .Subscribe(operation =>
                    {
                        responseInfo = new ResponseInfo(webRequest);
                        if (!responseInfo.isError && responseInfo.isComplete)
                        {
                            emitter.OnNext(UserResponseDto.Parse(responseInfo.response.response));
                            emitter.OnCompleted();
                        }
                        else if (responseInfo.isError)
                        {
                            emitter.OnError(new Exception(responseInfo.response.error));
                            emitter.OnCompleted();

                        }
                        else
                        {
                            throw new TimeoutException("server response not received");
                        }
                    });
            });
        }

        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}