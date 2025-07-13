using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DMS.Entrypoint.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ServiceUrls _serviceUrls;

        public StatusController(
            ILogger<StatusController> logger,
            IHttpClientFactory httpClientFactory,
            ServiceUrls serviceUrls)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _serviceUrls = serviceUrls;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var result = new Dictionary<string, string>
            {
                { "status", "OK" },
                { "timestamp", DateTime.UtcNow.ToString("o") },
                { "serviceUrls", $"CRM: {_serviceUrls.CRM}, Inventory: {_serviceUrls.Inventory}, Sales: {_serviceUrls.Sales}, Service: {_serviceUrls.Service}, Parts: {_serviceUrls.Parts}, Reporting: {_serviceUrls.Reporting}" }
            };

            return Ok(result);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServiceStatus()
        {
            var serviceStatus = new Dictionary<string, string>();

            try
            {
                var crmClient = _httpClientFactory.CreateClient("CrmService");
                var crmResponse = await crmClient.GetAsync("/health");
                serviceStatus.Add("crm", crmResponse.IsSuccessStatusCode ? "UP" : "DOWN");
            }
            catch (Exception ex)
            {
                serviceStatus.Add("crm", $"ERROR: {ex.Message}");
            }

            // Similar checks for other services...

            return Ok(serviceStatus);
        }
    }
}
