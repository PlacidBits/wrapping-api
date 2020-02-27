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
    /// <inheritdoc />
    public class ApiClient : IApiClient
    {
        const string AUTH_REQUEST_CONTENT = "grant_type=password&username=username&password=password";
        private const string TOKEN_ENDPOINT_URL = "token";
        private readonly HttpClient _apiClient;
        private string _apiBaseAddress;

        /// <inheritdoc />
        public ApiEnvironment Environment { get; private set; }

        public ApiClient(ApiEnvironment environment, string apiBaseAddress = null)
        {
            ValidateEnvironment(environment, apiBaseAddress);

            _apiClient = new HttpClient();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.SetEnvironment(environment);
        }

        /// <inheritdoc />
        public void SetEnvironment(ApiEnvironment environment, string apiBaseAddress = null)
        {
            ValidateEnvironment(environment, apiBaseAddress);

            switch (environment)
            {
                case ApiEnvironment.Dev:
                    _apiBaseAddress = apiBaseAddress;
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

        /// <summary>
        /// Checks the environment and base address for validity
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="apiBaseAddress"></param>
        private void ValidateEnvironment(ApiEnvironment environment, string apiBaseAddress)
        {
            if (environment == ApiEnvironment.Dev && string.IsNullOrWhiteSpace(apiBaseAddress))
                throw new ArgumentNullException("When targeting Local, you must provide an apiBaseAddress", nameof(apiBaseAddress));
            else if (environment == ApiEnvironment.Dev && !apiBaseAddress.EndsWith("/"))
                throw new ArgumentException("apiBaseAddress must end with a trailing slash.", nameof(apiBaseAddress));
        }

        /// <summary>
        /// Retrieves just the bearer access token for inclusion in HTTP requests. 
        /// </summary>
        /// <remarks>
        /// Exposed for convenience and special use cases. Operational calls utilize this method internally to prepare HTTP requests.
        /// </remarks>
        /// <returns>String representation of the access_token</returns>
        private async Task<string> GetAccessTokenAsync()
        {
            var token = await GetAccessTokenResponseAsync().ConfigureAwait(false);
            return token.Access_Token;
        }

        /// <summary>
        /// Retrieves the bearer access token object. 
        /// </summary>
        /// <remarks>
        /// Exposed for convenience and special use cases. Operational calls utilize this method internally to prepare HTTP requests.
        /// </remarks>
        /// <returns><see cref="OAuthTokenResponse"/> object</returns>
        private async Task<OAuthTokenResponse> GetAccessTokenResponseAsync()
        {
            var response = await _apiClient.PostAsync(_apiBaseAddress + TOKEN_ENDPOINT_URL, new StringContent(AUTH_REQUEST_CONTENT, Encoding.UTF8, "text/plain")).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw new ApiSecurityException(string.Format("Unable to obtain security token. Received a {0} response. Reason: {1}", response.StatusCode, response.ReasonPhrase));

            return await response.Content.ReadAsAsync<OAuthTokenResponse>().ConfigureAwait(false);
        }

        /// <summary>
        /// Replaces the access token being used by the API client
        /// </summary>
        /// <remarks>
        /// Use this if you don't want or need to create a brand new HTTP client, but need to continue using it past the token expiration
        /// </remarks>        
        private async Task RefreshAccessTokenAsync()
        {
            var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);
            _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <inheritdoc />
        public Task<HealthCheckResponseDto> CheckHealth(HealthCheckRequestDto dto = null) => 
            new HealthCheckOperation(_apiClient, _apiBaseAddress, this.RefreshAccessTokenAsync)
                .Call(dto);
    }
}
