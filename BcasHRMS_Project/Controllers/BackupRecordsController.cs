using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackupRecordsController : ControllerBase
    {
        private readonly tblBackupRecordsService _tblBackupRecordsService = new tblBackupRecordsService();

        [HttpGet]
        public async Task<IActionResult> GetAlltblBackupRecords()
        {
            try
            {
                var data = await _tblBackupRecordsService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblBackupRecords(int id)
        {
            try
            {
                var data = await _tblBackupRecordsService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> InserttblRefEducationalLevel([FromBody] tblBackupRecords tblBackupRecords)
        {
            try
            {
                var data = await _tblBackupRecordsService.Insert(tblBackupRecords);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblBackupRecords(int id, [FromBody] tblBackupRecords tblBackupRecords)
        {
            try
            {
                if (id != tblBackupRecords.BackupID) return BadRequest("Id mismatched.");

                var data = await _tblBackupRecordsService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _tblBackupRecordsService.Update(tblBackupRecords);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblRefEducationalLevel(int id)
        {
            try
            {
                var data = await _tblBackupRecordsService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblBackupRecordsService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
