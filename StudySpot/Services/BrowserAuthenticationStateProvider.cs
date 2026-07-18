// Client/Services/BrowserAuthenticationStateProvider.cs
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

public class BrowserAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public BrowserAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        
        var identity = new ClaimsIdentity();
        if (!string.IsNullOrEmpty(token))
        {
            identity = CreateIdentity(token);
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task NotifyUserAuthenticationAsync(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        var identity = CreateIdentity(token);
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private static ClaimsIdentity CreateIdentity(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        return new ClaimsIdentity(
            token.Claims,
            "jwt",
            ClaimTypes.Email,
            ClaimTypes.Role);
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}