using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Web_Registration.Dtos;

namespace Web_Registration.Pages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty] public string Login { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string DateOfBirth { get; set; } = "";

    public bool Success { get; set; }
    public string Message { get; set; } = "";

    //Вызывается автоматически когда юзер ввел данные и подтвердил
    public async Task OnPostAsync()
    {
        var dto = new RegisterRequest
        {
            Login = Login,
            Email = Email,
            Password = Password,
            DateOfBirth = DateOfBirth
        };

        var json = JsonSerializer.Serialize(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Важно: страницу обслуживает тот же сервер, поэтому берём относительный URL
        // Но HttpClient требует абсолютный — сделаем через текущий host:
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var url = $"{baseUrl}/api/auth/register";

        //отправка запроса в Api
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsync(url, content);

        var responseText = await response.Content.ReadAsStringAsync();

        ApiResponse? api;
        try
        {
            api = JsonSerializer.Deserialize<ApiResponse>(responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });//нужно, чтобы не зависеть от регистра (success vs Success).
        }
        catch
        {
            api = null;
        }

        Success = response.IsSuccessStatusCode && api?.Success == true;
        Message = api?.Message ?? responseText;
    }
}
