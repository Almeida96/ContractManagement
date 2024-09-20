using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContractManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Aqui vamos usar um exemplo básico de login com credenciais fixas.
        // Em um cenário real, você buscaria essas credenciais no banco de dados.
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // Validação simples das credenciais (você pode usar Identity ou um banco de dados aqui)
            if (login.Username == "admin" && login.Password == "password")
            {
                // Criação do token JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("Kf8eTg7Hw9LdVc3pQr6Mx1BnAz4Xy5Wz"); // Use a mesma chave que você definiu no Program.cs
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, login.Username) }),
                    Expires = DateTime.UtcNow.AddHours(2), // Token válido por 2 horas
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Retorna o token JWT para o cliente
                return Ok(new { Token = tokenString });
            }

            // Caso as credenciais estejam incorretas
            return Unauthorized();
        }
    }

    // Modelo de login para receber as credenciais do usuário
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
