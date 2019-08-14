using PerfectWardApi.Api;
using System.Net;
using System.Net.Http;

namespace PerfectWardAPI.Api
{
    public static class ProxyCredentials
    {
        private static string _username = string.Empty;
        private static string _password = string.Empty;

        static ProxyCredentials()
        {
            _username = EnvironmentVariables.Get(EnvironmentVariables.ProxyUsername);
            _password = EnvironmentVariables.Get(EnvironmentVariables.ProxyPassword);
        }

        public static HttpClientHandler CreateHandler()
        {
            var proxy = WebRequest.GetSystemWebProxy();
            proxy.Credentials = new NetworkCredential(_username, _password);
            return new HttpClientHandler() { Proxy = proxy };
        }

        public static HttpClientHandler CreateHandler(NetworkCredential nc)
        {
            _username = nc.UserName;
            _password = nc.Password;
            return CreateHandler();
        }
    }
}