using System;
using System.Net.Http.Headers;

namespace PerfectWardAPI.Api
{
    public class StringCredentials : IApiCredentials
    {
        private readonly string _username;
        private readonly string _apiToken;

        public StringCredentials(string username, string apiToken)
        {
            _username = username;
            _apiToken = apiToken;

            if (_username == null || _apiToken == null)
                throw new ArgumentNullException();
        }

        public void Authenticate(HttpRequestHeaders hrh)
        {
            hrh.Clear();
            hrh.Add("X-User-Email", _username);
            hrh.Add("X-User-Token", _apiToken);
            hrh.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
