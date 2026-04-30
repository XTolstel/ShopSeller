using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;

class Program_3
{
    public class UserData
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    static void Main(string[] args)
    {
        var userData = RunWebApp();
        Console.WriteLine("Данные получены!");
        Console.WriteLine("Email: " + userData.Email);
        Console.WriteLine("Password: " + userData.Password);

        // Продолжение работы программы с userData
    }

    static UserData RunWebApp()
    {
        var tcs = new TaskCompletionSource<UserData>();
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet("/", () =>
            Results.Content($$"""
<html>
  <head>
    <style>
      body {
        display: flex;
        flex-direction: column;
        align-items: center;
        height: 100vh;
        margin: 0;
        background-color: #f0f0f0;
      }
      .header {
        margin-top: 12.5vh;
        font-size: 20px;
        font-family: Arial, sans-serif;
        font-weight: bold;
        margin-bottom: 30px;
      }
      .container {
        margin-top: 12.5vh;
      }
      form {
        background: white;
        padding: 20px;
        border-radius: 10px;
        box-shadow: 0 0 10px rgba(0,0,0,0.2);
        display: flex;
        flex-direction: column;
        gap: 10px;
      }
      input {
        padding: 8px;
        font-size: 14px;
      }
      button {
        padding: 8px;
        font-size: 14px;
        cursor: pointer;
      }
      p {
        text-align: center;
      }
    </style>
  </head>
  <body>
    <div class="header">
      Hello, please write your email and password for registration
    </div>
    <div class="container">
      <form method='post' action='/save'>
        <input type='email' name='userEmail' placeholder='Введите email' required />
        <input type='password' name='userPassword' placeholder='Введите пароль' required />
        <button type='submit'>Сохранить</button>
      </form>
    </div>
  </body>
</html>
""", "text/html; charset=utf-8")
        );

        app.MapPost("/save", async (HttpRequest request) =>
        {
            var form = await request.ReadFormAsync();
            var data = new UserData
            {
                Email = form["userEmail"],
                Password = form["userPassword"]
            };

            tcs.SetResult(data); // Возвращаем данные в основной поток
            return Results.Redirect("/list");
        });

        app.MapGet("/list", () =>
            Results.Content("<html><body><a href='/'>Назад</a></body></html>", "text/html; charset=utf-8")
        );

        // Запуск сервера в отдельном потоке
        //var serverThread = new Thread(() => app.Run());
        // serverThread.IsBackground = true;
        //serverThread.Start();
        app.Run();
        // Ждём, пока пользователь отправит данные
        return tcs.Task.Result;
    }
}
