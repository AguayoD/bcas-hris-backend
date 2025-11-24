using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploymentStatusController : BaseController
    {
        private readonly tblEmploymentStatusService _employmentStatusService = new tblEmploymentStatusService();

        public EmploymentStatusController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

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

                // Log the INSERT action
                if (data?.EmploymentStatusID != null)
                {
                    await LogActionAsync("tblEmploymentStatus", "INSERT", data.EmploymentStatusID.ToString(), null, data);
                }

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

                var oldData = await _employmentStatusService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _employmentStatusService.Update(employmentStatus);

                // Log the UPDATE action
                await LogActionAsync("tblEmploymentStatus", "UPDATE", id.ToString(), oldData, updatedData);

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

                // Log the DELETE action
                await LogActionAsync("tblEmploymentStatus", "DELETE", id.ToString(), data, null);

                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}