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
    public class RoleController : ControllerBase
    {
        private readonly tblUserRoleService _roleService;

        public RoleController(tblUserRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/role
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

        // GET: api/role/5
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

        // POST: api/role
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
                return CreatedAtAction(nameof(GetById), new { id = createdRole.RoleId }, createdRole);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/role/5
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
                return Ok(updatedRole);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/role/5
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
                return NoContent();
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}