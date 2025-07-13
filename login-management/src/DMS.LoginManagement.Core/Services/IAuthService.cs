using DMS.LoginManagement.Core.DTOs;

namespace DMS.LoginManagement.Core.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    Task<RefreshTokenResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<UserInfoDto?> RegisterAsync(RegisterDto registerDto);
    Task<bool> LogoutAsync(int userId, string? refreshToken = null);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendEmailVerificationAsync(string email);
}

public interface ITokenService
{
    string GenerateAccessToken(int userId, string email, string role);
    string GenerateRefreshToken();
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<bool> RevokeAllRefreshTokensAsync(int userId);
    int? GetUserIdFromToken(string token);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateSecureToken();
}
