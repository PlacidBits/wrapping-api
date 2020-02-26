using API.Client.Exceptions;
using API.Client.Operations;
using API.Client.Security;
using API.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace API.Client
{
    public class ApiClient : IApiClient
    {
        const string AUTH_REQUEST_CONTENT = "grant_type=password&username=username&password=password";
        private const string TOKEN_ENDPOINT_URL = "token";
        private readonly HttpClient _apiClient;
        private string _apiBaseAddress;

        public ApiEnvironment Environment { get; private set; }

        public ApiClient(ApiEnvironment environment)
        {
            _apiClient = new HttpClient();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.SetEnvironment(environment);
        }

        public void SetEnvironment(ApiEnvironment environment)
        {
            switch (environment)
            {
                case ApiEnvironment.Dev:
                    _apiBaseAddress = ApiClientEnvironmentUrl.Dev;
                    break;
                case ApiEnvironment.Test:
                    _apiBaseAddress = ApiClientEnvironmentUrl.Test;
                    break;
                case ApiEnvironment.Beta:
                    _apiBaseAddress = ApiClientEnvironmentUrl.Beta;
                    break;
                case ApiEnvironment.Prod:
                    _apiBaseAddress = ApiClientEnvironmentUrl.Prod;
                    break;
                default:
                    throw new ArgumentException("Unrecognized environment", nameof(environment));
            }

            this.Environment = environment;
            this.RefreshAccessTokenAsync().GetAwaiter().GetResult();
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var token = await GetAccessTokenResponseAsync().ConfigureAwait(false);
            return token.Access_Token;
        }

        public async Task<OAuthTokenResponse> GetAccessTokenResponseAsync()
        {
            var response = await _apiClient.PostAsync(_apiBaseAddress + TOKEN_ENDPOINT_URL, new StringContent(AUTH_REQUEST_CONTENT, Encoding.UTF8, "text/plain")).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new ApiSecurityException(string.Format("Unable to obtain security token. Received a {0} response. Reason: {1}", response.StatusCode, response.ReasonPhrase));

            return await response.Content.ReadAsAsync<OAuthTokenResponse>().ConfigureAwait(false);
        }

        public async Task RefreshAccessTokenAsync()
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public Task<HealthCheckResponseDto> CheckHealth(HealthCheckRequestDto dto = null) => new HealthCheckOperation(_apiClient, _apiBaseAddress, this.RefreshAccessTokenAsync).Call(dto);
    }
}
