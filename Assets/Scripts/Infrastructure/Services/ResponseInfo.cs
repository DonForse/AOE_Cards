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
        public string responseString;
        public ResponseDto response;

        public ResponseInfo(UnityWebRequest webRequest)
        {
            isComplete = webRequest.isDone;
            code = webRequest.responseCode;
            isError = code >= 400 || webRequest.isNetworkError;
            if (webRequest.downloadHandler == null)
                return;
            
            responseString = isError ?
                              webRequest.error
                              : isComplete ? Encoding.UTF8.GetString(webRequest.downloadHandler.data, 3, webRequest.downloadHandler.data.Length - 3)
                              : string.Empty;

            response = JsonUtility.FromJson<ResponseDto>(responseString);
            
            //todo: change server to user this responsedto for all requests
            // isError = isError || !string.IsNullOrWhiteSpace(response.error);
            if (isError)
                Debug.LogWarning(responseString + ". "+  response.error);
            else
                Debug.Log(responseString + ". "+ response.response);
                
        }

        public class ResponseDto
        {
            public string error;
            public string response;
        }
    }
}