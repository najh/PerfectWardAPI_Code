using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace PerfectWardAPI.Api
{
    public class IniCredentials : IApiCredentials
    {
        private readonly string USERNAME;
        private readonly string APITOKEN;

        public IniCredentials(string path)
        {
            foreach (var iniLine in
                File.ReadAllLines(path)
                .Where(x => !x.StartsWith(";"))
                .Select(x => x.Split('='))
                .Where(x => x.Length == 2))
            {
                var val = iniLine[1].Trim();
                switch (iniLine[0].Trim().ToUpper())
                {
                    case nameof(USERNAME):
                        USERNAME = val;
                        break;
                    case nameof(APITOKEN):
                        APITOKEN = val;
                        break;
                }
            }

            if (USERNAME == null || APITOKEN == null)
                throw new ArgumentNullException();
        }

        public void Authenticate(HttpRequestHeaders hrh)
        {
            hrh.Clear();
            hrh.Add("X-User-Email", USERNAME);
            hrh.Add("X-User-Token", APITOKEN);
            hrh.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
