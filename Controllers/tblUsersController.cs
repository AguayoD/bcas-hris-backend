using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.UsersDTO;
using Repositories.Services;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class tblUsersController : ControllerBase
    {
        private readonly tblUsersService _tbluserService = new tblUsersService();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByUserId(int id)
        {
            try
            {
                var data = await _tbluserService.GetByIdWithRoles(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertUser([FromBody] UserInsertDTO userDTO)
        {
            try
            {
                var currentUser = await _tbluserService.GetByUsername(userDTO.Username);
                if (currentUser != null)
                {
                    throw new Exception("Username exists, please create a new one.");
                }
                var newUser = await _tbluserService.Insert(userDTO);
                return Ok(newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
