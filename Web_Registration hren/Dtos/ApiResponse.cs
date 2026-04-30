using Web_Registration.Services;
namespace Web_Registration.Dtos;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public UserDto user { get; set; }      // данные пользователя
}
public class UserDto
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string Email { get; set; } = "";
    public String DateOfBirth { get; set; } = "";

    public int balance { get; set; }

    public int spendbalance { get; set; }
}


