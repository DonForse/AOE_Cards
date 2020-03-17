using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services
{
    public class MatchService : IMatchService
    {
        private static readonly HttpClient Client = new HttpClient();
        private const string BaseUrl = "";
        private string StartMatchUrl => BaseUrl + "/games/users/{0}/matches";

        private string PlayTurnUrl => BaseUrl + "/games/users/{0}/matches/{1}/play/{2}";

        public async Task<MatchStatus> StartMatch()
        {
            var values = new Dictionary<string, string>
            {
                {"thing1", "hello"},
                {"thing2", "world"}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await Client.PostAsync(StartMatchUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return DtoToMatchStatus(responseString);
        }

        private MatchStatus DtoToMatchStatus(string responseString)
        {
            return JsonUtility.FromJson<MatchStatus>(responseString);
        }

        public void PlayEventCard(string cardName)
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
