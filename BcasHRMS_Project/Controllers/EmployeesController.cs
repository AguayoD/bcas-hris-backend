using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly tblEmployeeService _tblEmployeeService = new tblEmployeeService();

        [HttpGet]
        public async Task<IActionResult> GetAlltblEmployees()
        {
            try
            {
                var data = await _tblEmployeeService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblEmployees(int id)
        {
            try
            {
                var data = await _tblEmployeeService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> InserttblEmployees([FromBody] tblEmployees tblEmployees)
        {
            try
            {
                var data = await _tblEmployeeService.Insert(tblEmployees);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblEmployees(int id, [FromBody] tblEmployees tblEmployees)
        {
            try
            {
                if (id != tblEmployees.EmployeeID) return BadRequest("Id mismatched.");

                var data = await _tblEmployeeService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _tblEmployeeService.Update(tblEmployees);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblEmployees(int id)
        {
            try
            {
                var data = await _tblEmployeeService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblEmployeeService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
