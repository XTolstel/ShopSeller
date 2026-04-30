using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Registration.Dtos;
using Web_Registration.Services;

namespace Web_Registration.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly BirthDateValidator _birthDateValidator;
    private readonly WriteDbService _writeDBservice;

    public AuthController(BirthDateValidator birthDateValidator, WriteDbService writeService)
    {
        _birthDateValidator = birthDateValidator;
        _writeDBservice = writeService;


    }

    [HttpPost("login")]
    public ActionResult<ApiResponse> Login([FromBody] LoginRequest req)
    {
        // лёгкая нормализация
        var email = req.Email.Trim();
        var password = req.Password.Trim();

        // можно добавить простую проверку на пустые значения
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Email and password are required."
            });
        }

        var res = _writeDBservice.Login(email, password);

        return res.Success ? Ok(res) : Unauthorized(res);
    }
    [HttpPost("register")]
    public ActionResult<ApiResponse> Register([FromBody] RegisterRequest req)
    {
        // лёгкая нормализация
        var userName = req.Login.Trim();
        var email = req.Email.Trim();
        var birthDate = req.DateOfBirth.Trim();

        if (!_birthDateValidator.TryValidate(birthDate, out var error))
            return BadRequest(new ApiResponse { Success = false, Message = error });

        var res = _writeDBservice.Register(userName, email, req.Password, birthDate);

        return res.Success ? Ok(res) : BadRequest(res);
    }
}

