using DMS.SettingsManagement.Core.Entities;

namespace DMS.SettingsManagement.Core.Services
{
    public interface ISettingsService
    {
        Task<IEnumerable<Setting>> GetAllSettingsAsync();
        Task<Setting?> GetSettingAsync(string key);
        Task<IEnumerable<Setting>> GetSettingsByCategoryAsync(string category);
        Task<Setting> CreateSettingAsync(Setting setting);
        Task<Setting> UpdateSettingAsync(Setting setting);
        Task<bool> DeleteSettingAsync(string key);
        Task<bool> SettingExistsAsync(string key);
        Task<IEnumerable<string>> GetCategoriesAsync();
    }
}
