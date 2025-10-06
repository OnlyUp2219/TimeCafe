using System.ComponentModel.DataAnnotations;

namespace MVC.Auth.TimeCafe.API.Models;

public class PhoneVerificationModel
{
    [Required(ErrorMessage = "Введите номер телефона")]
    [Phone(ErrorMessage = "Некорректный номер")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите код")]
    public string Code { get; set; } = string.Empty; 

    public string Token { get; set; } = string.Empty;
}

