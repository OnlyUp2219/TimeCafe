using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.TimeCafe.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ExternalAuthController : ControllerBase
	{
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IConfiguration _configuration;

		public ExternalAuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IConfiguration configuration)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_configuration = configuration;
		}

		[HttpGet("challenge/{provider}")]
		[AllowAnonymous]
		public IActionResult ChallengeExternal([FromRoute] string provider, [FromQuery] string returnUrl = "/external/callback")
		{
			var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, Url.ActionLink(nameof(Callback), values: new { returnUrl }));
			return Challenge(props, provider);
		}

		[HttpGet("callback")]
		[AllowAnonymous]
		public async Task<IActionResult> Callback([FromQuery] string? returnUrl)
		{
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return Unauthorized();
			}

			var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
			IdentityUser user;
			if (signInResult.Succeeded)
			{
				user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
			}
			else
			{
				var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? $"{info.ProviderKey}@{info.LoginProvider}.local";
				user = await _userManager.FindByEmailAsync(email);
				if (user == null)
				{
					user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
					var createRes = await _userManager.CreateAsync(user);
					if (!createRes.Succeeded) return BadRequest(new { errors = createRes.Errors.Select(e => e.Description) });
				}
				var addLoginRes = await _userManager.AddLoginAsync(user, info);
				if (!addLoginRes.Succeeded) return BadRequest(new { errors = addLoginRes.Errors.Select(e => e.Description) });
			}

			var token = GenerateJwt(user, out var expiresInSeconds);

			if (!string.IsNullOrEmpty(returnUrl))
			{
				var redirectUrl = $"{returnUrl}#access_token={Uri.EscapeDataString(token)}&expires_in={expiresInSeconds}&token_type=Bearer";
				return Redirect(redirectUrl);
			}

			return Ok(new { access_token = token, expires_in = expiresInSeconds, token_type = "Bearer" });
		}

		private string GenerateJwt(IdentityUser user, out int expiresInSeconds)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var issuer = jwtSection["Issuer"]!;
			var audience = jwtSection["Audience"]!;
			var signingKey = jwtSection["SigningKey"]!;

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.Id),
				new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new Claim(ClaimTypes.NameIdentifier, user.Id)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(15);
			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: expires,
				signingCredentials: creds);

			expiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds;
			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
} 