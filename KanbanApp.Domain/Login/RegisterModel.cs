
namespace KanbanApp.Domain.Login
{
    using System.ComponentModel.DataAnnotations;
    using KanbanApp.Domain.User;
  public class RegisterModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; } = UserRole.User;
}
}