// Controllers/RoleController.cs
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BCAS_HRMSbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {
        private readonly tblUserRoleService _roleService;

        public RoleController(tblUserRoleService roleService, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<ActionResult<tblRoles>> Create([FromBody] tblRoles role)
        {
            try
            {
                if (role == null)
                {
                    return BadRequest("Role object is null");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdRole = await _roleService.Insert(role);

                // Log the INSERT action
                if (createdRole?.RoleId != null)
                {
                    await LogActionAsync("tblRoles", "INSERT", createdRole.RoleId.ToString(), null, createdRole);
                }

                return CreatedAtAction(nameof(GetById), new { id = createdRole.RoleId }, createdRole);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<tblRoles>> Update(int id, [FromBody] tblRoles role)
        {
            try
            {
                if (role == null)
                {
                    return BadRequest("Role object is null");
                }

                if (id != role.RoleId)
                {
                    return BadRequest("Role ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingRole = await _roleService.GetById(id);
                if (existingRole == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                var updatedRole = await _roleService.Update(role);

                // Log the UPDATE action
                await LogActionAsync("tblRoles", "UPDATE", id.ToString(), existingRole, updatedRole);

                return Ok(updatedRole);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existingRole = await _roleService.GetById(id);
                if (existingRole == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }

                await _roleService.DeleteById(id);

                // Log the DELETE action
                await LogActionAsync("tblRoles", "DELETE", id.ToString(), existingRole, null);

                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET methods remain the same
        [HttpGet]
        public async Task<ActionResult<IEnumerable<tblRoles>>> GetAll()
        {
            try
            {
                var roles = await _roleService.GetAll();
                return Ok(roles);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<tblRoles>> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetById(id);
                if (role == null)
                {
                    return NotFound($"Role with ID {id} not found");
                }
                return Ok(role);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}