using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly tblDepartmentService _tblDepartmentService = new tblDepartmentService();

        [HttpGet]
        public async Task<IActionResult> GetAlltblDepartment()
        {
            try
            {
                var data = await _tblDepartmentService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblDepartment(int id)
        {
            try
            {
                var data = await _tblDepartmentService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> InserttblDepartment([FromBody] tblDepartment tblDepartment)
        {
            try
            {
                var data = await _tblDepartmentService.Insert(tblDepartment);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblDepartment(int id, [FromBody] tblDepartment tblDepartment)
        {
            try
            {
                if (id != tblDepartment.DepartmentID) return BadRequest("Id mismatched.");

                var data = await _tblDepartmentService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _tblDepartmentService.Update(tblDepartment);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblDepartment(int id)
        {
            try
            {
                var data = await _tblDepartmentService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblDepartmentService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
