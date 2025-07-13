using AutoMapper;
using DMS.SettingsManagement.Core.DTOs;
using DMS.SettingsManagement.Core.Entities;
using DMS.SettingsManagement.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DMS.SettingsManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IMapper _mapper;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISettingsService settingsService, IMapper mapper, ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettingDto>>> GetAllSettings()
        {
            try
            {
                var settings = await _settingsService.GetAllSettingsAsync();
                var settingDtos = _mapper.Map<IEnumerable<SettingDto>>(settings);
                return Ok(settingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all settings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{key}")]
        public async Task<ActionResult<SettingDto>> GetSetting(string key)
        {
            try
            {
                var setting = await _settingsService.GetSettingAsync(key);
                if (setting == null)
                {
                    return NotFound($"Setting with key '{key}' not found");
                }

                var settingDto = _mapper.Map<SettingDto>(setting);
                return Ok(settingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setting with key {Key}", key);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<SettingDto>>> GetSettingsByCategory(string category)
        {
            try
            {
                var settings = await _settingsService.GetSettingsByCategoryAsync(category);
                var settingDtos = _mapper.Map<IEnumerable<SettingDto>>(settings);
                return Ok(settingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                var categories = await _settingsService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SettingDto>> CreateSetting([FromBody] CreateSettingDto createSettingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingSetting = await _settingsService.SettingExistsAsync(createSettingDto.Key);
                if (existingSetting)
                {
                    return Conflict($"Setting with key '{createSettingDto.Key}' already exists");
                }

                var setting = _mapper.Map<Setting>(createSettingDto);
                var createdSetting = await _settingsService.CreateSettingAsync(setting);
                var settingDto = _mapper.Map<SettingDto>(createdSetting);

                return CreatedAtAction(nameof(GetSetting), new { key = settingDto.Key }, settingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating setting");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{key}")]
        public async Task<ActionResult<SettingDto>> UpdateSetting(string key, [FromBody] UpdateSettingDto updateSettingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingSetting = await _settingsService.GetSettingAsync(key);
                if (existingSetting == null)
                {
                    return NotFound($"Setting with key '{key}' not found");
                }

                _mapper.Map(updateSettingDto, existingSetting);
                existingSetting.Key = key; // Ensure key doesn't change
                
                var updatedSetting = await _settingsService.UpdateSettingAsync(existingSetting);
                var settingDto = _mapper.Map<SettingDto>(updatedSetting);

                return Ok(settingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting with key {Key}", key);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{key}")]
        public async Task<ActionResult> DeleteSetting(string key)
        {
            try
            {
                var deleted = await _settingsService.DeleteSettingAsync(key);
                if (!deleted)
                {
                    return NotFound($"Setting with key '{key}' not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting setting with key {Key}", key);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow, service = "DMS Settings Management API" });
        }
    }
}
