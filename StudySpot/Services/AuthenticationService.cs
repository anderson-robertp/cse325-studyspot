using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StudySpot.Data;
using StudySpot.Models;
using Microsoft.EntityFrameworkCore;

public class AuthenticationService
{
    private readonly IConfiguration _config;
    private readonly StudySpotContext _context;

    public AuthenticationService(IConfiguration config, StudySpotContext context)
    {
        _config = config;
        _context = context;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
          new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
          new Claim(ClaimTypes.Email, user.Email),
          new Claim(ClaimTypes.Role, user.Role)  
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<User?> AuthenticateSync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        // Validation checks
        if (string.IsNullOrWhiteSpace(firstName))
            return (false, "First name is required.", null);

        if (string.IsNullOrWhiteSpace(lastName))
            return (false, "Last name is required.", null);

        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required.", null);

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.", null);

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters long.", null);

        if (password != confirmPassword)
            return (false, "Passwords do not match.", null);

        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (existingUser != null)
            return (false, "An account with this email already exists.", null);

        // Validate email format
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                return (false, "Invalid email address.", null);
        }
        catch
        {
            return (false, "Invalid email address.", null);
        }

        // Create new user
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "user",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "User registered successfully.", user);
    }
}