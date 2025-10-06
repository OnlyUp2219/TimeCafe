using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVC.Auth.TimeCafe.API.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MVC.Auth.TimeCafe.API.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class VerifyCodeModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    public VerifyCodeModel(IConfiguration configuration, UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        _accountSid = configuration["Twilio:AccountSid"] ?? "";
        _authToken = configuration["Twilio:AuthToken"] ?? "";
        _twilioPhoneNumber = configuration["Twilio:TwilioPhoneNumber"] ?? "";
    }

    [BindProperty]
    public PhoneVerificationModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? UserEnteredCode { get; set; }



    public async Task<IActionResult> OnGetAsync(string phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);

        if (string.IsNullOrEmpty(Input.Token))
        {
            Input.PhoneNumber = phoneNumber;
            Input.Token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            var random = new Random();
            var smsCode = random.Next(100000, 999999).ToString();
            Input.Code = smsCode;
        }

        TempData["Token"] = Input.Token;
        TempData["Code"] = Input.Code;


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Input.Code = TempData["Code"] as string ?? "";
        if (user == null)
            return RedirectToPage("/Account/Login");

        if (Input.PhoneNumber == null || Input.Token == null)
        {
            ModelState.AddModelError("", "Что-то пошло не так. Попробуйте снова.");
        TempData.Keep();

            return Page();
        }

        if (UserEnteredCode != Input.Code)
        {
            ModelState.AddModelError("", "Неверный код.");
            TempData.Keep();

            return Page();
        }

        var result = await _userManager.ChangePhoneNumberAsync(user, Input.PhoneNumber, Input.Token);

        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Телефон успешно подтверждён.";
            return RedirectToPage("/Account/Manage/Index");
        }

        ModelState.AddModelError("", "Ошибка подтверждения номера.");


        return Page();
    }

    public async Task<IActionResult> OnGetGenerateSmsAsync(string phoneNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }
        var random = new Random();
        var smsCode = random.Next(100000, 999999).ToString();
        Input.PhoneNumber = phoneNumber;
        Input.Code = smsCode;

        //await GenerateSms(phoneNumber, user);
        Input.Token = TempData["Token"] as string;
        TempData["Code"] = Input.Code;
        TempData.Keep();

        return Page();
    }


    private async Task GenerateSms(string phoneNumber, IdentityUser user)
    {
        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        var code = token;

        TwilioClient.Init(_accountSid, _authToken);

        var message = await MessageResource.CreateAsync(
            body: $"Ваш код подтверждения: {code}",
            from: new Twilio.Types.PhoneNumber(_twilioPhoneNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );

        if (message.ErrorCode == null)
        {
            Input.PhoneNumber = phoneNumber;
            Input.Code = code;
            Input.Token = token;
        }
    }
}
