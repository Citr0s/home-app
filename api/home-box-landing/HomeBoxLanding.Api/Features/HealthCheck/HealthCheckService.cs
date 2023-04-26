using System.Net;
using HomeBoxLanding.Api.Features.HealthCheck.Types;

namespace HomeBoxLanding.Api.Features.HealthCheck;

public class HealthCheckService
{
    private readonly HttpClient _httpClient;

    public HealthCheckService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public HealthCheckResponse PerformHealthCheck(string url, bool isSecure)
    {
        var prefix = isSecure ? "https" : "http";

        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(2);
            var result = _httpClient.GetAsync($"{prefix}://{url}").Result;
            var responseMessage = result.Content.ReadAsStringAsync().Result;

            return new HealthCheckResponse
            {
                StatusCode = result.StatusCode,
                StatusDescription = result.ReasonPhrase
            };
        }
        catch (Exception e)
        {
            if (isSecure)
            {
                return new HealthCheckResponse
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    StatusDescription = e.Message
                };
            }
                
            return new HealthCheckResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                StatusDescription = e.Message
            };
        }
    }
}