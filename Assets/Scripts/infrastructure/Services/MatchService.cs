using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services
{
    public class MatchService : IMatchService
    {
        HttpWebRequest webRequest;
        
        private const string BaseUrl = "";
        private string StartMatchUrl => BaseUrl + "/games/users/{0}/matches";

        private string PlayTurnUrl => BaseUrl + "/games/users/{0}/matches/{1}/play/{2}";

        public void StartMatch(string playerId, Action<MatchStatus> onStartMatchComplete)
        {
            var values = new Dictionary<string, string>
            {
                {"thing1", "hello"},
                {"thing2", "world"}
            };
            var content = new FormUrlEncodedContent(values);
            var url = "http://www.google.com";
            //var url =string.Format(StartMatchUrl, playerId);
            webRequest = WebRequest.Create(url) as HttpWebRequest;
            
            webRequest.BeginGetResponse(FinishWebRequest, onStartMatchComplete);
        }
        void FinishWebRequest(IAsyncResult result)
        {
            var webResponse = webRequest.EndGetResponse(result);
            string responseString;
            using (Stream stream = webResponse.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    responseString = reader.ReadToEnd();
                }
            }

            if (result.AsyncState == null)
                return;
            
            var callback = (Action<MatchStatus>) result.AsyncState;
            callback(DtoToMatchStatus(responseString));
        }

        private void OnGetResponse(Task<string> obj)
        {
            throw new NotImplementedException();
        }

        private MatchStatus DtoToMatchStatus(string responseString)
        {
            return new MatchStatus();
            return JsonUtility.FromJson<MatchStatus>(responseString);
        }

        public void PlayUpgradeCard(string cardName)
        {
            throw new System.NotImplementedException();
        }

        public void PlayUnitCard(string cardName)
        {
            throw new System.NotImplementedException();
        }
    }
}

/* HTTP WEB REQUEST IF C# Version Errors.
 * var request = (HttpWebRequest)WebRequest.Create("http://www.example.com/recepticle.aspx");

var postData = "thing1=" + Uri.EscapeDataString("hello");
postData += "&thing2=" + Uri.EscapeDataString("world");
var data = Encoding.ASCII.GetBytes(postData);

request.Method = "POST";
request.ContentType = "application/x-www-form-urlencoded";
request.ContentLength = data.Length;

using (var stream = request.GetRequestStream())
{
stream.Write(data, 0, data.Length);
}

var response = (HttpWebResponse)request.GetResponse();

var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
 */
