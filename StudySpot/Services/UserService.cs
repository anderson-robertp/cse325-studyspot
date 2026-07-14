using StudySpot.Models;
using System.Net.Http.Json;
using StudySpot.DTOs;
using Microsoft.EntityFrameworkCore;
using StudySpot.Data;

namespace StudySpot.Services;

public class UserService
{
    private readonly StudySpotContext _context;

    public UserService(StudySpotContext context)
    {
        _context = context;
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        try
        {
            return await _context.Users.FindAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            return await _context.Users.FindAsync(id);
        }
        catch
        {
            return null;
        }
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
            .ToListAsync() ?? new List<UserListDto>();
    }

    public async Task UpdateUserAsync(Guid id, User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> UpdateEmailAsync(UpdateEmailRequest request)
    {
        var response = await _context.PutAsJsonAsync("api/users/me/email", request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(message) ? "Unable to update email right now." : message);
        }

        return (true, "Email updated successfully.");
    }

    public async Task<(bool Success, string Message)> UpdatePasswordAsync(UpdatePasswordRequest request)
    {
        var response = await _http.PutAsJsonAsync("api/users/me/password", request);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(message) ? "Unable to update password right now." : message);
        }

        return (true, "Password updated successfully.");
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/users/{id}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> GetUserCountAsync()
    {
        var response = await _http.GetAsync("api/users/count");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }
}