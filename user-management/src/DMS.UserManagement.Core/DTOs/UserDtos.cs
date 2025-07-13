using System.ComponentModel.DataAnnotations;

namespace DMS.UserManagement.Core.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class CreateUserDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Phone { get; set; }
    
    [MaxLength(255)]
    public string? Department { get; set; }
}

public class UpdateUserDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Phone { get; set; }
    
    [MaxLength(255)]
    public string? Department { get; set; }
}

public class ChangePasswordDto
{
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
