using Microsoft.AspNetCore.Mvc;
using Auth.TimeCafe.API.Services;

namespace Auth.TimeCafe.API.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateAdminRequest request)
    {
        var (success, errors) = await _adminService.CreateAdminAsync(request.Email, request.Password);
        if (!success)
            return BadRequest(new { errors });

        return Ok(new { message = "Admin created successfully." });
    }
}

public record CreateAdminRequest(string Email, string Password);
