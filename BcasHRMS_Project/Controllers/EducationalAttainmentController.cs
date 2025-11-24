using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationalAttainmentController : BaseController
    {
        private readonly tblEducationalAttainmentService _educationalAttainmentService = new tblEducationalAttainmentService();

        public EducationalAttainmentController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEducationalAttainment()
        {
            try
            {
                var data = await _educationalAttainmentService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdEducationalAttainment(int id)
        {
            try
            {
                var data = await _educationalAttainmentService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertEducationalAttainment([FromBody] tblEducationalAttainment educationalAttainment)
        {
            try
            {
                var data = await _educationalAttainmentService.Insert(educationalAttainment);

                // Log the INSERT action
                if (data?.EducationalAttainmentID != null)
                {
                    await LogActionAsync("tblEducationalAttainment", "INSERT", data.EducationalAttainmentID.ToString(), null, data);
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEducationalAttainment(int id, [FromBody] tblEducationalAttainment educationalAttainment)
        {
            try
            {
                if (id != educationalAttainment.EducationalAttainmentID) return BadRequest("Id mismatched.");

                var oldData = await _educationalAttainmentService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _educationalAttainmentService.Update(educationalAttainment);

                // Log the UPDATE action
                await LogActionAsync("tblEducationalAttainment", "UPDATE", id.ToString(), oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdEducationalAttainment(int id)
        {
            try
            {
                var data = await _educationalAttainmentService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _educationalAttainmentService.DeleteById(id);

                // Log the DELETE action
                await LogActionAsync("tblEducationalAttainment", "DELETE", id.ToString(), data, null);

                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}