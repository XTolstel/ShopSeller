using Web_Registration.Dtos;
//using Write;
using static Write.WriteDB;
namespace Web_Registration.Services;


    public class WriteDbService
    {


    public ApiResponse Login(string email, string password)
        {

            string result = Write.WriteDB.Login(email, password);

            // У тебя в старой логике были текстовые ответы.
            // Мы упакуем их в JSON.
            bool ok =
                !result.Contains("not found", StringComparison.OrdinalIgnoreCase) &&
                !result.Contains("Invalid", StringComparison.OrdinalIgnoreCase) &&
                !result.Contains("error", StringComparison.OrdinalIgnoreCase);
        var user = Write.WriteDB.GetUserByEmail(email);
        if (!ok)
        {
            user = null;
        }
        return new ApiResponse
        {
            Success = ok,
            Message = result,
            user = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                    balance = 0,
                    spendbalance = 0
            }
        };
        }

    public ApiResponse Register(string userName, string email, string password, string birthDate)
        {
            // Здесь мы используем твою существующую БД-логику.
            // Поля названий я привёл к типичному виду — если у тебя отличаются,
            // скажешь, и я подгоню 1:1 под твой класс.
            var data = new Write.WriteDB.User
            {
                Login = userName,
                Email = email,
                Password = password,
                DateOfBirth = birthDate
            };

            string result = Write.WriteDB.Write_toDB(data);

            // У тебя в старой логике были текстовые ответы.
            // Мы упакуем их в JSON.
            bool ok =
                !result.Contains("already", StringComparison.OrdinalIgnoreCase) &&
                !result.Contains("error", StringComparison.OrdinalIgnoreCase);

            return new ApiResponse
            {
                Success = ok,
                Message = result
            };
        }
    }
