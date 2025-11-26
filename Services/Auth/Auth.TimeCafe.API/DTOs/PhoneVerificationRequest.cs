using System.ComponentModel.DataAnnotations;

namespace Auth.TimeCafe.API.DTOs;

public class PhoneVerificationRequest
{
    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код")]
    public string Code { get; set; } = string.Empty;

    public string? CaptchaToken { get; set; }
}
