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

    [HttpGet("client-edit-function")]
    [Authorize]
    public IActionResult ClientEditFunction()
    {
        return Ok("Эта функция доступна авторизованным пользователям!");
    }
}