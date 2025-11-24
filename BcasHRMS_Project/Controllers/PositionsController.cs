// Controllers/PositionsController.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : BaseController
    {
        private readonly tblPositionsService _tblPositionsService = new tblPositionsService();

        public PositionsController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAlltblPositions()
        {
            try
            {
                var data = await _tblPositionsService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblPositions(int id)
        {
            try
            {
                var data = await _tblPositionsService.GetById(id);
                if (data == null) return NoContent();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InserttblPositions([FromBody] tblPositions tblPositions)
        {
            try
            {
                var data = await _tblPositionsService.Insert(tblPositions);

                // Log the INSERT action
                if (data?.PositionID != null)
                {
                    await LogActionAsync("tblPositions", "INSERT", data.PositionID.ToString(), null, data);
                }

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblPositions(int id, [FromBody] tblPositions tblPositions)
        {
            try
            {
                if (id != tblPositions.PositionID) return BadRequest("Id mismatched.");

                var oldData = await _tblPositionsService.GetById(id);
                if (oldData == null) return NotFound();

                var updatedData = await _tblPositionsService.Update(tblPositions);

                // Log the UPDATE action
                await LogActionAsync("tblPositions", "UPDATE", id.ToString(), oldData, updatedData);

                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblPositions(int id)
        {
            try
            {
                var data = await _tblPositionsService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblPositionsService.DeleteById(id);

                // Log the DELETE action
                await LogActionAsync("tblPositions", "DELETE", id.ToString(), data, null);

                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}