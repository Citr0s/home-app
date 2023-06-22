using HomeBoxLanding.Api.Features.HealthCheck.Types;
using Microsoft.AspNetCore.Mvc;

namespace HomeBoxLanding.Api.Features.HealthCheck;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthCheckService _service;

    public HealthCheckController()
    {
        _service = new HealthCheckService(new HttpClient());
    }

    [HttpGet]
    public async Task<HealthCheckResponse> Get([FromQuery] string url, [FromQuery] bool isSecure)
    {
        return await _service.PerformHealthCheck(url, isSecure).ConfigureAwait(false);
    }
}