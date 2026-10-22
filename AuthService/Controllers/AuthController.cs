using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.Name);

        if (user == null)
            return Unauthorized(new { message = "Invalid username." });

        if (user.PasswordHash != request.Password)
            return Unauthorized(new { message = "Invalid password." });

        return Ok(new
        {
            message = "Login successful!",
            token = "fake-jwt-token-12345",
            name = user.Name,
            role = user.Role // <-- This needs to be here!
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Name == request.Name);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username is already taken." });
        }

        var newUser = new User
        {
            Name = request.Name,
            PasswordHash = request.Password,
            Role = "Customer"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully!" });
    }
}

public class LoginRequest
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}