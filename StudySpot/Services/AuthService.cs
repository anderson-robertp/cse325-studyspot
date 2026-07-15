using Microsoft.EntityFrameworkCore;
using StudySpot.Data;
using StudySpot.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace StudySpot.Services;

public class AuthService
{
    private readonly StudySpotContext _context;
    private readonly AuthenticationService _authenticationService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        StudySpotContext context,
        AuthenticationService authenticationService,
        AuthenticationStateProvider authStateProvider)
    {
        _context = context;
        _authenticationService = authenticationService;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse> LoginAsync(
        string email,
        string password)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(
                password,
                user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var token = _authenticationService.GenerateToken(user);

            if (_authStateProvider is BrowserAuthenticationStateProvider provider)
            {
                await provider.NotifyUserAuthenticationAsync(token);
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = token
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Login error: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword)
    {
        try
        {
            if (password != confirmPassword)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Passwords do not match."
                };
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email address is already registered."
                };
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var token = _authenticationService.GenerateToken(user);

            if (_authStateProvider is BrowserAuthenticationStateProvider provider)
            {
                await provider.NotifyUserAuthenticationAsync(token);
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful.",
                Token = token
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Registration error: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        if (_authStateProvider is BrowserAuthenticationStateProvider provider)
        {
            await provider.NotifyUserLogoutAsync();
        }
    }
}