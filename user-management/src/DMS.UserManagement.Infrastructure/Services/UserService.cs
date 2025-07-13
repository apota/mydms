using AutoMapper;
using DMS.UserManagement.Core.DTOs;
using DMS.UserManagement.Core.Entities;
using DMS.UserManagement.Core.Services;
using DMS.UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DMS.UserManagement.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManagementDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public UserService(
        UserManagementDbContext context,
        IMapper mapper,
        IPasswordService passwordService,
        IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _passwordService = passwordService;
        _configuration = configuration;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == createUserDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = _passwordService.HashPassword(createUserDto.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        // Check if email is being changed and if it already exists
        if (user.Email != updateUserDto.Email)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == updateUserDto.Email && u.Id != id);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists");
            }
        }

        _mapper.Map(updateUserDto, user);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.PasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
