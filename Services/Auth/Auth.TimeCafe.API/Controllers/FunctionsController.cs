using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.TimeCafe.API.Controllers;

[ApiController]
[Route("[controller]")]
public class FunctionsController : ControllerBase
{
    [HttpGet("public-function")]
    [Authorize]
    public IActionResult PublicFunction()
    {
        return Ok("Эта функция доступна всем авторизованным пользователям!");
    }

    [HttpGet("admin-function")]
    [Authorize(Roles = "admin")] 
    public IActionResult AdminFunction()
    {
        return Ok("Эта функция доступна только администраторам!");
    }
}