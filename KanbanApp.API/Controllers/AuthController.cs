using Microsoft.AspNetCore.Mvc;
using KanbanApp.Domain.Login;
using KanbanApp.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.API.Middleware;
using System.Security.Claims;

namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var result = await _authService.RegisterAsync(model);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 401)]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _authService.LoginAsync(model);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _authService.DeleteUserAsync(id, currentUserId);
        return NoContent();
    }
}