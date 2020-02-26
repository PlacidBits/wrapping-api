using API.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Client
{
    public interface IApiClient
    {
        ApiEnvironment Environment { get; }

        Task<HealthCheckResponseDto> CheckHealth(HealthCheckRequestDto dto = null);
    }
}
