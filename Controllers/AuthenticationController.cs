using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.UsersDTO;
using Repositories.Services;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        AuthenticationService _authenticateService = new AuthenticationService();

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateUser([FromBody] UserLoginDTO userLogin)
        {
            try
            {
                var user = await _authenticateService.UserLogin(userLogin);
                if (user == null)
                {
                    return BadRequest("Invalid Username or Password.");
                }
                var token = _authenticateService.GenerateToken(user);
                return Ok(new
                {
                    Token = token,
                    User = user,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
