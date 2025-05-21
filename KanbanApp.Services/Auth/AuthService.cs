using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using KanbanApp.Domain.Login;
using KanbanApp.Domain.User;
using KanbanApp.API.Exceptions;
namespace KanbanApp.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterModel model)
    {
        if (await _userRepository.GetByUsernameAsync(model.Username) != null)
            throw new RegistrationException("Пользователь с таким именем уже существует");

        if (await _userRepository.GetByEmailAsync(model.Email) != null)
            throw new RegistrationException("Пользователь с такой почтой уже существует");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
           model.Password,
           BCrypt.Net.BCrypt.GenerateSalt(12)
        );

        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            Role = model.Role
        };

        await _userRepository.AddAsync(user);

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginModel model)
    {
        // Ищем только по email
        var user = await _userRepository.GetByEmailAsync(model.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            throw new AuthenticationException("Неверная почта или пароль");

        return new AuthResponse
        {
            Token = GenerateJwtToken(user),
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

        var credentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

     public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task DeleteUserAsync(string userIdToDelete, string currentUserId)
{
    // Проверяем, что пользователь существует
    var userToDelete = await _userRepository.GetByIdAsync(userIdToDelete);
    if (userToDelete == null)
    {
        throw new ArgumentException("User not found");
    }

    // Запрещаем удаление самого себя
    if (userToDelete.Id == currentUserId)
    {
        throw new InvalidOperationException("You cannot delete yourself");
    }

    await _userRepository.DeleteAsync(userToDelete);
}
}