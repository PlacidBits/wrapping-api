using API.Client.Exceptions;
using Polly;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.Client.Operations
{
    abstract class BaseOperation<TRequest, TResponse>
    {
        private readonly Func<Task> _refreshAccessTokenAsync;
        protected HttpClient ApiClient { get; private set; }
        protected string ApiBaseAddress { get; private set; }

        public BaseOperation(HttpClient apiClient, string apiBaseAddress, Func<Task> refreshAccessTokenAsync)
        {
            ApiClient = apiClient;
            ApiBaseAddress = apiBaseAddress;
            _refreshAccessTokenAsync = refreshAccessTokenAsync;
        }

        public abstract Task<TResponse> Call(TRequest dto);

        protected async Task<TResponse> ExecuteAsync(Task<HttpResponseMessage> apiCall)
        {
            var policy = Policy
                .Handle<ApiCallException>()
                .RetryAsync(async (ex, retryCount, context) =>
                {
                    await _refreshAccessTokenAsync().ConfigureAwait(false);
                });

            return await policy.ExecuteAsync(async () =>
            {
                var response = await apiCall.ConfigureAwait(false);
                this.ValidateResponse(response);
                return await response.Content.ReadAsAsync<TResponse>().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        protected virtual void ValidateResponse(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException reqEx)
            {
                throw new ApiCallException("Encountered a problem while trying to communicate with Belle Tire API.", reqEx);
            }
        }
    }
}
