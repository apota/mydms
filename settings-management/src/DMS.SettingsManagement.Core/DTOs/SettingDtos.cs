using System.ComponentModel.DataAnnotations;

namespace DMS.SettingsManagement.Core.DTOs
{
    public class CreateSettingDto
    {
        [Required]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Category { get; set; } = "General";
        
        public string DataType { get; set; } = "string";
        
        public bool IsUserEditable { get; set; } = true;
    }
    
    public class UpdateSettingDto
    {
        [Required]
        public string Value { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Category { get; set; } = "General";
        
        public string DataType { get; set; } = "string";
        
        public bool IsUserEditable { get; set; } = true;
    }
    
    public class SettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "General";
        public string DataType { get; set; } = "string";
        public bool IsUserEditable { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
