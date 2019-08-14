using System.Net.Http.Headers;

namespace PerfectWardAPI.Api
{
    public interface IApiCredentials
    {
        void Authenticate(HttpRequestHeaders hrh);
    }
}
