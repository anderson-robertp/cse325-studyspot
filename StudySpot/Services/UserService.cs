using Microsoft.EntityFrameworkCore;
using StudySpot.Data;
using StudySpot.DTOs;
using StudySpot.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace StudySpot.Services;



public class UserService
{
    private readonly StudySpotContext _context;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public UserService(
        StudySpotContext context,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _context = context;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var authenticationState =
            await _authenticationStateProvider
                .GetAuthenticationStateAsync();

        var principal = authenticationState.User;

        var userIdClaim = principal.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }

        return await _context.Users
            .FindAsync(userId);
    }   

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users
            .FindAsync(id);
    }

    public async Task<List<UserListDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(user => new UserListDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            })
            .ToListAsync();
    }

    public async Task UpdateUserAsync(Guid id, User user)
    {
        var existingUser = await _context.Users
            .FindAsync(id);

        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;

        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> UpdateEmailAsync(
        UpdateEmailRequest request)
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            return (false, "User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return (false, "Email address cannot be empty.");
        }

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Email == request.Email &&
                u.UserId != user.UserId);

        if (emailExists)
        {
            return (false, "That email address is already in use.");
        }

        user.Email = request.Email;

        await _context.SaveChangesAsync();

        return (true, "Email updated successfully.");
    }

    public async Task<(bool Success, string Message)> UpdatePasswordAsync(
        UpdatePasswordRequest request)
    {
        var user = await GetCurrentUserAsync();

        if (user == null)
        {
            return (false, "User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return (false, "New passwords do not match.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return (false, "New password cannot be empty.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
        request.NewPassword);

        await _context.SaveChangesAsync();

        return (true, "Password updated successfully.");
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _context.Users
            .FindAsync(id);

        if (user == null)
        {
            return;
        }

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }
}