using AutoMapper;
using DMS.LoginManagement.Core.DTOs;
using DMS.LoginManagement.Core.Entities;
using DMS.LoginManagement.Core.Services;
using DMS.LoginManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DMS.LoginManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly LoginManagementDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(
        LoginManagementDbContext context,
        IPasswordService passwordService,
        ITokenService tokenService,
        IMapper mapper,
        IConfiguration configuration)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Status == "Active");

        if (user == null || !_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7"))
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        var userInfo = _mapper.Map<UserInfoDto>(user);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userInfo,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60"))
        };
    }

    public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (tokenEntity == null || 
            tokenEntity.IsRevoked || 
            tokenEntity.ExpiresAt <= DateTime.UtcNow ||
            tokenEntity.User.Status != "Active")
        {
            return null;
        }

        // Revoke the old refresh token
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(
            tokenEntity.User.Id, 
            tokenEntity.User.Email, 
            tokenEntity.User.Role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = tokenEntity.User.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7"))
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        return new RefreshTokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "60"))
        };
    }

    public async Task<UserInfoDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = _mapper.Map<User>(registerDto);
        user.PasswordHash = _passwordService.HashPassword(registerDto.Password);
        user.EmailVerificationToken = _passwordService.GenerateSecureToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserInfoDto>(user);
    }

    public async Task<bool> LogoutAsync(int userId, string? refreshToken = null)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        }
        else
        {
            await _tokenService.RevokeAllRefreshTokensAsync(userId);
        }

        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Revoke all refresh tokens to force re-login
        await _tokenService.RevokeAllRefreshTokensAsync(userId);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

        if (user == null) return true; // Don't reveal if email exists

        user.PasswordResetToken = _passwordService.GenerateSecureToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send email with reset link
        // await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email &&
                                    u.PasswordResetToken == resetPasswordDto.Token &&
                                    u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null) return false;

        user.PasswordHash = _passwordService.HashPassword(resetPasswordDto.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Revoke all refresh tokens
        await _tokenService.RevokeAllRefreshTokensAsync(user.Id);

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token &&
                                    u.EmailVerificationTokenExpiry > DateTime.UtcNow);

        if (user == null) return false;

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendEmailVerificationAsync(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.EmailVerified);

        if (user == null) return false;

        user.EmailVerificationToken = _passwordService.GenerateSecureToken();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // TODO: Send verification email
        // await _emailService.SendEmailVerificationAsync(user.Email, user.EmailVerificationToken);

        return true;
    }
}
