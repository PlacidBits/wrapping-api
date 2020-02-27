using System;
using System.Collections.Generic;
using System.Text;

namespace API.Client
{
    static class ApiClientEnvironmentUrl
    {
        public static string Test => "https://test.myapi.com/";

        public static string Beta => "https://beta.myapi.com/";

        public static string Prod => "https://myapi.com/";
    }

    public enum ApiEnvironment
    {
        Dev,
        Test,
        Beta,
        Prod
    }
}
