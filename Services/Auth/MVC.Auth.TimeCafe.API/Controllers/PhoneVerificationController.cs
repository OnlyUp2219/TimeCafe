using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC.Auth.TimeCafe.API.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;


namespace MVC.Auth.TimeCafe.API.Controllers;

public class PhoneVerificationController : Controller
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;
    private readonly UserManager<IdentityUser> _userManager;


    public PhoneVerificationController(IConfiguration configuration, UserManager<IdentityUser> userManager)
    {
        _accountSid = configuration["Twilio:AccountSid"] ?? "";
        _authToken = configuration["Twilio:AuthToken"] ?? "";
        _twilioPhoneNumber = configuration["Twilio:TwilioPhoneNumber"]?? "";
        _userManager = userManager;
    }
    private async Task GenerateSms(string phoneNumber, IdentityUser user)
    {
        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        // этот токен и есть твой SMS-код
        var code = token;

        TwilioClient.Init(_accountSid, _authToken);

        var message = await MessageResource.CreateAsync(
            body: $"Ваш код подтверждения: {code}",
            from: new Twilio.Types.PhoneNumber(_twilioPhoneNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber)
        );

        if (message.ErrorCode == null)
        {
            TempData["PhoneNumber"] = phoneNumber;
            TempData["Code"] = code;
            TempData["PhoneToken"] = token;
            TempData.Keep();
        }
    }

}


