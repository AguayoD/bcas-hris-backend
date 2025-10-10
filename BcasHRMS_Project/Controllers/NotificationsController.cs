using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly tblNotificationsService _tblNotificationsService = new tblNotificationsService();

        [HttpGet]
        public async Task<IActionResult> GetAlltblNotifications()
        {
            try
            {
                var data = await _tblNotificationsService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdtblNotifications(int id)
        {
            try
            {
                var data = await _tblNotificationsService.GetById(id);
                if (data == null) return NoContent();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> InserttblNotifications([FromBody] tblNotifications tblNotifications)
        {
            try
            {
                var data = await _tblNotificationsService.Insert(tblNotifications);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatetblNotifications(int id, [FromBody] tblNotifications tblNotifications)
        {
            try
            {
                if (id != tblNotifications.NotificationID) return BadRequest("Id mismatched.");

                var data = await _tblNotificationsService.GetById(id);
                if (data == null) return NotFound();

                var updatedData = await _tblNotificationsService.Update(tblNotifications);
                return Ok(updatedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteByIdtblNotifications(int id)
        {
            try
            {
                var data = await _tblNotificationsService.GetById(id);
                if (data == null) return NotFound();

                var deletedData = await _tblNotificationsService.DeleteById(id);
                return Ok(deletedData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
