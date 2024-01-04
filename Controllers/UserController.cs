using Authantication.Data.Context;
using Authantication.Data.Models;
using Authantication.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authantication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userMnager;


        public UserController(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userMnager = userManager;
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<ActionResult> Register(RegisterDTO register)
        {
            var newUser = new User
            {
                UserName = register.UserName,
                Departmnet = register.Department,
                Email = register.Email,

            };

            var result = await _userMnager.CreateAsync(newUser, register.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim("Departmnet", newUser.Departmnet),
                new Claim("Id", newUser.Id),
            };

            await _userMnager.AddClaimsAsync(newUser, userClaims);

            /*if (!createdClaims.Succeeded)
            {
                await _userMnager.DeleteAsync(newUser);
                return BadRequest("plesae try again");
            }*/

            return Ok("user created");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<TokenDTO>> Login (LoginDTO loginDTO)
        {
            var user = await _userMnager.FindByNameAsync(loginDTO.UserName);
            if (user == null)
            {
                return Unauthorized("Wrong Username or password");
            }

            var isLocked = await _userMnager.IsLockedOutAsync(user);
            if (isLocked)
            {
                return Unauthorized("Please try again later");
            }

            var userPassword = await _userMnager.CheckPasswordAsync(user, loginDTO.Password);
            if (!userPassword)
            {
                await _userMnager.AccessFailedAsync(user);
                return Unauthorized("Wrong Username or password");
            }

            var userClaims = await _userMnager.GetClaimsAsync(user);

            var result = GenerateToken(userClaims.ToList(),Guid.NewGuid());
            user.Token = result.Token;
            user.RefreshToken = result.RefreshToken;
            user.RefreshTokenExpiryDate = DateTime.Now.AddMonths(1);
            await _userMnager.UpdateAsync(user);

            return Ok(result);
        }

        [HttpPost]
        [Route("Refresh-token")]
        public async Task<ActionResult<TokenDTO>> RefreshToken(RefreshTokenDTO refreshToken)
        {
            if (refreshToken == null)
            {
                return BadRequest("input is empty");
            }

            var token = new JwtSecurityTokenHandler().ReadJwtToken(refreshToken.Token);
            var username = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var user = await _userMnager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest("Invalid token");
            }
            if (user.Token != refreshToken.Token
                || user.RefreshToken != refreshToken.RefreshToken
                || user.RefreshTokenExpiryDate <= DateTime.Now)
            {
                return BadRequest("something went wrong");
            }

            var generateTokne = GenerateToken(token.Claims.ToList(), user.RefreshToken);
            user.Token = generateTokne.Token;
            user.RefreshTokenExpiryDate = DateTime.Now.AddMonths(1);
            var updatedResult = await _userMnager.UpdateAsync(user);
            if (!updatedResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"something went wrong");
            }

            return generateTokne;
        }

        private TokenDTO GenerateToken(List<Claim> UserCliam, Guid RefreshToken = new Guid())
        {
            var secretKey = _configuration.GetValue<string>("SecretKey");
            var secretKeyinByte = Encoding.ASCII.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(secretKeyinByte);

            var methodInGerateToken = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                claims: UserCliam,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: methodInGerateToken
                );

            var tokenHandler = new JwtSecurityTokenHandler();
            var resultToken = tokenHandler.WriteToken(jwt);

            return new TokenDTO
            {
                Token = resultToken,
                ExpireDate = jwt.ValidTo,
                RefreshToken = RefreshToken
            };
        }

        #region static-login
/*        [HttpPost]
        [Route("Login")]
        public ActionResult Login(LoginDTO credentials)
        {
            if (credentials.UserName == "Ali" && credentials.Password == "Pass")
            {
                var UserCliam = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, credentials.UserName),
                    new Claim(ClaimTypes.Email, $"{credentials.UserName}@gmail.com"),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var secretKey = _configuration.GetValue<string>("SecretKey");
                var secretKeyinByte = Encoding.UTF8.GetBytes(secretKey);
                var key = new SymmetricSecurityKey(secretKeyinByte);

                var methodInGerateToken = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                var jwt = new JwtSecurityToken(
                    claims: UserCliam,
                    expires: DateTime.Now.AddMinutes(15),
                    signingCredentials: methodInGerateToken
                    );

                var tokenHandler = new JwtSecurityTokenHandler();
                var resultToken = tokenHandler.WriteToken(jwt);

                return Ok(resultToken);
            }
            return Unauthorized();
        }*/
        #endregion
    }
}
