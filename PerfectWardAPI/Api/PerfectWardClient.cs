using PerfectWardAPI.Data;
using PerfectWardAPI.Model.Reports;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PerfectWardAPI.Api
{
    public class PerfectWardClient
    {
        private struct ApiResponse
        {
            internal string Body;
            internal HttpStatusCode Status;
        }

        private const string API_BASE = "https://app.perfectward.com";
        private const string API_REPORTS = API_BASE + "/api/reports";
        private const string API_TEST = API_REPORTS + "?page=1&per_page=1";
        private const string API_REPORTS_PAGE = API_REPORTS + "?page={0}&per_page=100";
        private const string API_REPORTS_PAGE_SINCE = API_REPORTS_PAGE + "&since={1}";

        private const int RETRY_LIMIT = 10;
        private const int RETRY_TIME = 60;   //Time in seconds.

        private static int RATE_LIMIT = (int)TimeSpan.FromSeconds(2).TotalMilliseconds;  //Seconds between requests.
        private SemaphoreSlim LOCK = new SemaphoreSlim(1);
        private DateTime _lastRequestTime = DateTime.Now.AddMilliseconds(-RATE_LIMIT);

        private HttpClient _httpClient;

        private IApiCredentials _credentials;

        public PerfectWardClient(IApiCredentials credentials)
        {
            _credentials = credentials;
            _httpClient = new HttpClient(ProxyCredentials.CreateHandler());
        }

        public PerfectWardClient(IApiCredentials credentials, NetworkCredential proxyCredentials)
        {
            _credentials = credentials;
            _httpClient = new HttpClient(ProxyCredentials.CreateHandler(proxyCredentials));
        }

        async private Task<ApiResponse> SendRequest(string endpoint, HttpMethod httpMethod)
        {
            await LOCK.WaitAsync();
            Debug.Log($"Querying endpoint: {endpoint}");
            var elapsedSeconds = (int)(DateTime.Now - _lastRequestTime).TotalMilliseconds;
            if (elapsedSeconds < RATE_LIMIT)
            {
                var sleepTime = RATE_LIMIT - elapsedSeconds;
                Debug.Log($"Sleeping for {sleepTime}ms");
                Thread.Sleep(sleepTime);
            }

            var retryCount = 0;
            HttpStatusCode code;
            string response;
            while (true)
            {
                try
                {
                    _credentials.Authenticate(_httpClient.DefaultRequestHeaders);
                    var request = new HttpRequestMessage(httpMethod, endpoint);
                    var resp = await _httpClient.SendAsync(request);
                    code = resp.StatusCode;
                    Debug.Log($"Status code:{resp.StatusCode}");
                    if (!resp.IsSuccessStatusCode) return new ApiResponse()
                    {
                        Body = null,
                        Status = resp.StatusCode
                    };
                    response = await resp.Content.ReadAsStringAsync();
                    break;
                }
                catch (Exception ex)
                {
                    Debug.Log($"Error sending HttpRequest:\n{ex}");
                    if (++retryCount < RETRY_LIMIT)
                    {
                        Debug.Log($"Sleeping for {RETRY_TIME} seconds...");
                        Thread.Sleep(1000 * RETRY_TIME);    //Sleep for a minute.
                    }
                    else
                    {
                        Debug.Log($"Sending the HttpRequest failed {RETRY_LIMIT} times consecutively. Terminating...");
                        Environment.Exit(0);
                    }
                }
            }

            //Debug.Log($"Received response:\n{response}\n");
            _lastRequestTime = DateTime.Now;
            LOCK.Release();
            return new ApiResponse()
            {
                Body = response,
                Status = code
            };
        }

        async private Task<string> SendGet(string endpoint)
        {
            return (await SendRequest(endpoint, HttpMethod.Get)).Body;
        }

        async public Task<bool> TestApi()
        {
            Debug.Log($"Testing API...");
            var res = await SendRequest(API_TEST, HttpMethod.Get);
            return res.Status == HttpStatusCode.OK;
        }

        async public Task<ReportsListResponse> ListReports(int page = 1)
        {
            var json = await SendGet(string.Format(API_REPORTS_PAGE, page));
            return JSON.GetObject<ReportsListResponse>(json);
        }

        async public Task<ReportsListResponse> ListReportsSince(long unixTimestamp, int page = 1)
        {
            var parameterised = string.Format(API_REPORTS_PAGE_SINCE, page, unixTimestamp);
            return JSON.GetObject<ReportsListResponse>(await SendGet(parameterised));
        }

        async public Task<ReportsListResponse> ListReportsSince(DateTime since, int page = 1)
        {
            return await ListReportsSince(since.ToTimeStamp(), page);
        }

        async public Task<DetailedReportResponse> ReportDetails(int id)
        {
            var json = await SendGet($"{API_REPORTS}/{id}");
            return JSON.GetObject<DetailedReportResponse>(json);
        }
    }
}
