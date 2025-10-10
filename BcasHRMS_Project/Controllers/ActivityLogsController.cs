using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogsController : ControllerBase
    {
        private readonly tblActivityLogsService _tblActivityLogsService = new tblActivityLogsService();

        [HttpGet]
        public async Task<IActionResult> GetAlltblActivityLogsService()
        {
            try
            {
                var data = await _tblActivityLogsService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblActivityLogsService(int id)
        {
            try
            {
                var data = await _tblActivityLogsService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblActivityLogsService(int id)
        {
            try
            {
                var data = await _tblActivityLogsService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblActivityLogsService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
