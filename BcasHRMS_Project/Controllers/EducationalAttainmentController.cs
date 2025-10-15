using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EducationalAttainmentController : ControllerBase
    {
        private readonly tblEducationalAttainmentService _educationalAttainmentService = new tblEducationalAttainmentService();

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

                var data = await _educationalAttainmentService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _educationalAttainmentService.Update(educationalAttainment);
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
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}