using System.Text;
using UnityEngine.Networking;

namespace Infrastructure.Services
{
    public class ResponseInfo
    {
        public bool isComplete;
        public bool isError;
        public long code;
        public string response;

        public ResponseInfo(UnityWebRequest webRequest)
        {
            isComplete = webRequest.isDone;
            code = webRequest.responseCode;
            isError = code >= 400 || webRequest.isNetworkError;
            if (webRequest.downloadHandler == null)
                return;

            response = isError ?
                              webRequest.error
                              : isComplete ? Encoding.UTF8.GetString(webRequest.downloadHandler.data, 3, webRequest.downloadHandler.data.Length - 3)
                              : string.Empty;
        }
    }
}