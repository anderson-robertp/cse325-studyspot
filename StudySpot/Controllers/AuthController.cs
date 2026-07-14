using Microsoft.AspNetCore.Mvc;
using StudySpot.Services;
using StudySpot.Models;
using StudySpot.Data;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthenticationService _authService;

    public AuthController(AuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.AuthenticateSync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new AuthResponse { Success = false, Message = "Invalid credentials" });

        var token = _authService.GenerateToken(user);
        return Ok(new AuthResponse
        {
            Success = true,
            Token = token,
            User = user
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, message, user) = await _authService.RegisterAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.ConfirmPassword
        );

        if (!success)
            return BadRequest(new AuthResponse { Success = false, Message = message });

        var token = _authService.GenerateToken(user!);
        return Ok(new AuthResponse
        {
            Success = true,
            Message = message,
            Token = token,
            User = user
        });
    }
}