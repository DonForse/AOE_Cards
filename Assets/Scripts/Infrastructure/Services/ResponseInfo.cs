using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class ResponseInfo
    {
        public bool isComplete;
        public bool isError;
        public long code;
        public ResponseDto response;

        public ResponseInfo(UnityWebRequest webRequest)
        {
            isComplete = webRequest.isDone;
            code = webRequest.responseCode;
            isError = code >= 400 || webRequest.isNetworkError;
            if (webRequest.downloadHandler == null)
                return;
            if (isError || !webRequest.isDone) {
                response = new ResponseDto();
                response.error = webRequest?.error;
                if (isError)
                    Debug.LogWarning(response.error);
                return;
            }
            var responseString =  Encoding.UTF8.GetString(webRequest.downloadHandler.data, 3, webRequest.downloadHandler.data.Length - 3);

            response = JsonUtility.FromJson<ResponseDto>(responseString);
            
             isError = isError || !string.IsNullOrWhiteSpace(response.error);
            if (isError)
                Debug.LogWarning(responseString + ". "+  response.error);
            else
                Debug.Log(response.response);
                
        }
    }
}