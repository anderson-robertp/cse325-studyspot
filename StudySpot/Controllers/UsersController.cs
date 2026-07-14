using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using StudySpot.Models;
using StudySpot.Data;
using StudySpot.DTOs;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly StudySpotContext _context;

    public UsersController(StudySpotContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IEnumerable<UserListDto>> Get()
    {
        return await _context.Users.Select(u => new UserListDto
        {
            UserId = u.UserId,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            IsActive = u.IsActive,
            Role = u.Role
        }).ToListAsync();

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    [HttpGet("me")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(parsedId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await _context.Users.CountAsync();
        return count;
    }

    [HttpPost]
    public async Task<ActionResult<User>> Post(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = user.UserId }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> Put(Guid id, User user)
    {
        if (id != user.UserId)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("me/email")]
    public async Task<ActionResult> UpdateEmail(UpdateEmailRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(parsedId);
        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Current password is incorrect.");
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim() && u.UserId != user.UserId);
        if (existingUser != null)
        {
            return BadRequest("An account with this email already exists.");
        }

        user.Email = request.Email.ToLower().Trim();
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("me/password")]
    public async Task<ActionResult> UpdatePassword(UpdatePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(parsedId);
        if (user == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return BadRequest("Password must be at least 8 characters long.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return BadRequest("Passwords do not match.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<User>> Delete(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}