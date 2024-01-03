using JWTAPI.configuration;
using JWTAPI.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthManagmentController : ControllerBase
    {
        private readonly ILogger<AuthManagmentController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _JwtConfig;
        public AuthManagmentController(
            ILogger<AuthManagmentController> logger,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor)
        {
            _logger = logger;
            _userManager = userManager;
            _JwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto request)
        {
            if(ModelState.IsValid)
            {
                var emailExist = await _userManager.FindByEmailAsync(request.Email);

                if (emailExist != null) return BadRequest("email already exist");

                var newUser = new IdentityUser()
                {
                    Email = request.Email,
                    UserName = request.Email
                };

                var isCreated = await _userManager.CreateAsync(newUser, request.Password);
                if(isCreated.Succeeded)
                {

                    // generate token
                    var token = GenerateJwtToken(newUser);
                    return Ok(new RegistrationRequestResponse()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(isCreated.Errors.Select(x => x.Description).ToList());
            }

            return BadRequest("Invalid request payload");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser == null) return BadRequest("Invalid authentication");
                var isPasswordValid = await _userManager.CheckPasswordAsync(existingUser, request.Password);
                if (isPasswordValid)
                {
                    var token = GenerateJwtToken(existingUser);
                    return Ok(new LoginRequestResponse()
                    {
                        Result = true,
                        Token = token
                    });
                }
                    
                return BadRequest("Password is incorrect");
            }

            return BadRequest("Invalid request payload");
        }


        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_JwtConfig.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512)
            };
           
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }
    }
}
