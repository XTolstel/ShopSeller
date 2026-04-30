using System.ComponentModel.DataAnnotations;

namespace Web_Registration.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(64)]
    public string Login { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public String DateOfBirth { get; set; }
}
