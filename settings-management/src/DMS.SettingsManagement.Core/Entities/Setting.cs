using System.ComponentModel.DataAnnotations;

namespace DMS.SettingsManagement.Core.Entities
{
    public class Setting
    {
        [Key]
        public string Key { get; set; } = string.Empty;
        
        [Required]
        public string Value { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string Category { get; set; } = "General";
        
        public string DataType { get; set; } = "string";
        
        public bool IsUserEditable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public string? CreatedBy { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
}
