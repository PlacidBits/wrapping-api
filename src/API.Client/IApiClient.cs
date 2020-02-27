using API.Client.Security;
using API.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Client
{
    /// <summary>
    /// Provides an OO abstraction over your API
    /// </summary>
    /// <remarks>
    /// As best practice, it is strongly recommended to keep one instance per app domain
    /// in order to adhere to the internally-tracked HTTP client's recommended usage
    /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netframework-4.5.1#remarks
    /// </remarks>
    public interface IApiClient
    {
        /// <summary>
        /// Represents the environment the client is currently pointed at
        /// </summary>
        ApiEnvironment Environment { get; }

        /// <summary>
        /// Retrieves health response
        /// </summary>
        Task<HealthCheckResponseDto> CheckHealth(HealthCheckRequestDto dto = null);
    }
}
