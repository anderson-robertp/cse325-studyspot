using StudySpot.Models;
using System.Net.Http.Json;

namespace StudySpot.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly Blazored.LocalStorage.ILocalStorageService _localStorage;
    private readonly Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, Blazored.LocalStorage.ILocalStorageService localStorage, Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequest { Email = email, Password = password };
            var response = await _http.PostAsJsonAsync("api/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse?.Token != null)
                {
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                    
                    // Notify auth state provider of the change
                    if (_authStateProvider is BrowserAuthenticationStateProvider provider)
                    {
                        await provider.NotifyUserAuthenticationAsync(authResponse.Token);
                    }
                }
                return authResponse ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            
            var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return errorResponse ?? new AuthResponse { Success = false, Message = "Login failed" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Login error: {ex.Message}" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        try
        {
            var registerRequest = new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };
            
            var response = await _http.PostAsJsonAsync("api/auth/register", registerRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse?.Token != null)
                {
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                    
                    // Notify auth state provider of the change
                    if (_authStateProvider is BrowserAuthenticationStateProvider provider)
                    {
                        await provider.NotifyUserAuthenticationAsync(authResponse.Token);
                    }
                }
                return authResponse ?? new AuthResponse { Success = false, Message = "No response from server" };
            }
            
            var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return errorResponse ?? new AuthResponse { Success = false, Message = "Registration failed" };
        }
        catch (Exception ex)
        {
            return new AuthResponse { Success = false, Message = $"Registration error: {ex.Message}" };
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        
        // Notify auth state provider of logout
        if (_authStateProvider is BrowserAuthenticationStateProvider provider)
        {
            await provider.NotifyUserLogoutAsync();
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
}
