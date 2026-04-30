using System.Globalization;
using System.Text.RegularExpressions;

namespace Web_Registration.Services;

public class BirthDateValidator
{
    private static int CalculateAge(DateTime birthDate, DateTime today)
    {
        int age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    public bool TryValidate(string birthDate, out string errorMessage)
    {
        errorMessage = string.Empty;

        // Формат dd.MM.yyyy
        string pattern = @"^\d{2}\.\d{2}\.\d{4}$";
        if (!Regex.IsMatch(birthDate, pattern))
        {
            errorMessage = "the date doesn't matches the format xx.xx.xxxx.";
            return false;
        }

        if (!DateTime.TryParseExact(
                birthDate,
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
        {
            errorMessage = "Error: this date doesn't exist.";
            return false;
        }

        int age = CalculateAge(parsed, DateTime.Today);
        if (age < 18)
        {
            errorMessage = $"Age: {age}. You aren't 18 years old.";
            return false;
        }

        return true;
    }
}
