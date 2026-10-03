using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        public readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        //get

        //getById

        //Post
        [HttpPost("register")]
        public async Task<ActionResult> register(Credentials credentials)
        {

            var user = new IdentityUser() 
            {UserName = credentials.Username};

            var result = await _userManager.CreateAsync(user, credentials.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return StatusCode(StatusCodes.Status201Created, new {message = "User creadted successfully"});

        }

        [HttpPost("login")]
        public async Task<ActionResult> login(Credentials credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.Username);

            if (user == null)
            {
                return Unauthorized("Incorrect Username or Password");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, credentials.Password);

            if (!passwordValid)
            {
                return Unauthorized("Incorrect Username or Password");
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var expireInMinutes = double.Parse(jwtSettings["ExpireInMinutes"]!);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireInMinutes),
                signingCredentials: signingCredentials
            );






            return Ok(new {token = new JwtSecurityTokenHandler().WriteToken(token)});

        }
        //Update

        //Delete

        
    }
}
