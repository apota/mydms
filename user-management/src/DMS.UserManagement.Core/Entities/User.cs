using System.ComponentModel.DataAnnotations;

namespace DMS.UserManagement.Core.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [Required, MaxLength(20)]
    public string Status { get; set; } = "active";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    [MaxLength(255)]
    public string? Phone { get; set; }
    
    [MaxLength(255)]
    public string? Department { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}
