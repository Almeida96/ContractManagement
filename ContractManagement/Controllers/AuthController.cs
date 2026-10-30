using ContractManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContractManagement.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(AuthSettings auth) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public IActionResult Login(LoginModel login)
    {
        if (!auth.Matches(login.Username, login.Password)) return Unauthorized();
        var expires = DateTime.UtcNow.AddHours(1);
        var token = new JwtSecurityToken(
            issuer: AuthSettings.Issuer, audience: AuthSettings.Audience,
            claims: [new Claim(ClaimTypes.Name, auth.Username)],
            expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key)), SecurityAlgorithms.HmacSha256));
        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token), ExpiresAt = expires });
    }
}
public record LoginModel(string Username, string Password);
