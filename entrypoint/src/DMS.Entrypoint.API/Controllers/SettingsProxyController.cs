using Microsoft.AspNetCore.Mvc;

namespace DMS.Entrypoint.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SettingsProxyController> _logger;

        public SettingsProxyController(IHttpClientFactory httpClientFactory, ILogger<SettingsProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SettingsService");
                var response = await client.GetAsync("/api/settings");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }
                
                return StatusCode((int)response.StatusCode, "Failed to fetch settings from settings service");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching settings from settings service");
                return StatusCode(500, "Internal server error while fetching settings");
            }
        }

        [HttpGet("health-check")]
        public async Task<IActionResult> CheckSettingsHealth()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SettingsService");
                var response = await client.GetAsync("/api/settings/health");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }
                
                return StatusCode((int)response.StatusCode, "Settings service health check failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking settings service health");
                return StatusCode(500, "Internal server error while checking settings service health");
            }
        }
    }
}
