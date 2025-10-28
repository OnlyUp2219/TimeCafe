using System.ComponentModel.DataAnnotations;

namespace Auth.TimeCafe.Domain.Models;

public class PhoneVerificationModel
{
    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код")]
    public string Code { get; set; } = string.Empty;
    
    public string? CaptchaToken { get; set; }
}

