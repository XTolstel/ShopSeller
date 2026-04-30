using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoSellerUltra.Login;
using System.Text.Json.Serialization;

namespace AutoSellerUltra.Api;

public class RegisterRequest
{
    // Имена свойств должны совпадать с твоим API:
    // login, email, password, dateOfBirth
    public string Login { get; set; } = "";

    public string Email { get; set; } = "";

    public string Password { get; set; } = "";

    public string DateOfBirth { get; set; } = "";
}
public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "";
}

public class ApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public UserDto user { get; set; }      // данные пользователя
}
