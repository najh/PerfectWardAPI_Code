using System;

namespace PerfectWardApi.Api
{
    public static class EnvironmentVariables
    {
        public const string SQLConnectionString = "PW_API_SQL";
        public const string PerfectWardEmail = "PW_API_EMAIL";
        public const string PerfectWardToken = "PW_API_TOKEN";
        public const string ProxyUsername = "PW_PXY_USER";
        public const string ProxyPassword = "PW_PXY_PASS";

        public static void Set(string variableName, string value)
        {
            Environment.SetEnvironmentVariable(variableName, value, EnvironmentVariableTarget.User);
        }

        public static string Get(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.User);
        }
    }
}
