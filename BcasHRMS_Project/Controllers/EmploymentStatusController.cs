using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentStatusController : ControllerBase
    {
        private readonly tblEmploymentStatusService _employmentStatusService = new tblEmploymentStatusService();

        [HttpGet]
        public async Task<IActionResult> GetAllEmploymentStatus()
        {
            try
            {
                var data = await _employmentStatusService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdEmploymentStatus(int id)
        {
            try
            {
                var data = await _employmentStatusService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertEmploymentStatus([FromBody] tblEmploymentStatus employmentStatus)
        {
            try
            {
                var data = await _employmentStatusService.Insert(employmentStatus);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEmploymentStatus(int id, [FromBody] tblEmploymentStatus employmentStatus)
        {
            try
            {
                if (id != employmentStatus.EmploymentStatusID) return BadRequest("Id mismatched.");

                var data = await _employmentStatusService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _employmentStatusService.Update(employmentStatus);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdEmploymentStatus(int id)
        {
            try
            {
                var data = await _employmentStatusService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _employmentStatusService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}