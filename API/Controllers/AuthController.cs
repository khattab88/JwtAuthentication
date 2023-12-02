using API.Config;
using API.Data;
using API.Dtos;
using API.Dtos.Requests;
using API.Dtos.Responses;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _db;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParameters,
            AppDbContext db
            )
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _tokenValidationParameters = tokenValidationParameters;
            _db = db;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var response = new RegisterResponse();
                response.Success = false;
                response.Errors.Add("Invalid payload");

                return BadRequest(response);
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null) 
            {
                var response = new RegisterResponse();
                response.Success = false;
                response.Errors.Add("Email already exists");

                return BadRequest(response);
            }

            var newUser = new IdentityUser() 
            {
                UserName = request.UserName,
                Email = request.Email,
            };

            var userCreated = await _userManager.CreateAsync(newUser, request.Password);

            if(!userCreated.Succeeded) 
            {
                var response = new RegisterResponse();
                response.Success = false;
                response.Errors = userCreated.Errors.Select(e => e.Description).ToList();

                return BadRequest(response);
            }
            else
            {
                var result = await GenerateJwtToken(newUser);

                return Ok(result);
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var response = new LoginResponse();
                response.Success = false;
                response.Errors.Add("Invalid payload");

                return BadRequest(response);
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if(existingUser == null)
            {
                var response = new LoginResponse();
                response.Success = false;
                response.Errors.Add("Invalid login");

                return NotFound(response);
            }

            var validPassword = await _userManager.CheckPasswordAsync(existingUser, request.Password);

            if (!validPassword)
            {
                var response = new LoginResponse();
                response.Success = false;
                response.Errors.Add("Invalid login");

                return NotFound(response);
            }
            else
            {
                var result = await GenerateJwtToken(existingUser);

                return Ok(result);
            }
        }


        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwt = jwtTokenHandler.WriteToken(token);


            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(32) + Guid.NewGuid().ToString(),
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            return new AuthResult()
            {
                Success = true,
                Token = jwt,
                RefreshToken = refreshToken.Token
            };
        }

        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(c => c[random.Next(length)])
                .ToArray());
        }
    }
}
