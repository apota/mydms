using DMS.SettingsManagement.Core.Entities;
using DMS.SettingsManagement.Core.Services;
using DMS.SettingsManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DMS.SettingsManagement.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly SettingsDbContext _context;

        public SettingsService(SettingsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Setting>> GetAllSettingsAsync()
        {
            return await _context.Settings.OrderBy(s => s.Category).ThenBy(s => s.Key).ToListAsync();
        }

        public async Task<Setting?> GetSettingAsync(string key)
        {
            return await _context.Settings.FindAsync(key);
        }

        public async Task<IEnumerable<Setting>> GetSettingsByCategoryAsync(string category)
        {
            return await _context.Settings
                .Where(s => s.Category == category)
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<Setting> CreateSettingAsync(Setting setting)
        {
            setting.CreatedAt = DateTime.UtcNow;
            setting.UpdatedAt = DateTime.UtcNow;
            
            _context.Settings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task<Setting> UpdateSettingAsync(Setting setting)
        {
            var existingSetting = await _context.Settings.FindAsync(setting.Key);
            if (existingSetting == null)
            {
                throw new KeyNotFoundException($"Setting with key '{setting.Key}' not found");
            }

            existingSetting.Value = setting.Value;
            existingSetting.Description = setting.Description;
            existingSetting.Category = setting.Category;
            existingSetting.DataType = setting.DataType;
            existingSetting.IsUserEditable = setting.IsUserEditable;
            existingSetting.UpdatedAt = DateTime.UtcNow;
            existingSetting.UpdatedBy = setting.UpdatedBy;

            await _context.SaveChangesAsync();
            return existingSetting;
        }

        public async Task<bool> DeleteSettingAsync(string key)
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting == null)
            {
                return false;
            }

            _context.Settings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SettingExistsAsync(string key)
        {
            return await _context.Settings.AnyAsync(s => s.Key == key);
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Settings
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
