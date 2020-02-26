using API.DTOs;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace API.Client.Operations
{
    class HealthCheckOperation : BaseOperation<HealthCheckRequestDto, HealthCheckResponseDto>
    {
        private const string ENDPOINT_URL = "health";

        public HealthCheckOperation(HttpClient apiClient, string apiBaseAddress, Func<Task> refreshAccessTokenAsync) : base(apiClient, apiBaseAddress, refreshAccessTokenAsync) { }

        public override Task<HealthCheckResponseDto> Call(HealthCheckRequestDto dto = null) => this.ExecuteAsync(this.ApiClient.GetAsync(this.ApiBaseAddress + ENDPOINT_URL));
    }
}
