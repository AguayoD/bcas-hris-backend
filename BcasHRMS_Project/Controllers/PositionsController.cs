using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Repositories.Service;
using Models.DTOs.UsersDTO;
using Models.Models;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : BaseController
    {
        private readonly tblPositionsService _tblPositionsService;
        private readonly TransactionEventService _transactionEventService;

        public PositionsController(
            IHttpContextAccessor httpContextAccessor,
            tblPositionsService tblPositionsService,
            TransactionEventService transactionEventService
        ) : base(httpContextAccessor)
        {
            _tblPositionsService = tblPositionsService;
            _transactionEventService = transactionEventService;
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

                if (data?.PositionID != null)
                {
                    var user = await _transactionEventService.GetCurrentUserAsync();
                    await LogTransactionEvent("CREATE", user, 0,
                        $"Added position: {data.PositionName}",
                        oldData: null, newData: data);
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

                var user = await _transactionEventService.GetCurrentUserAsync();
                string changes = GetChanges(oldData, updatedData);

                await LogTransactionEvent("UPDATE", user, 0,
                    $"Updated position: {updatedData.PositionName}",
                    oldData, updatedData);

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

                await _tblPositionsService.DeleteById(id);

                var user = await _transactionEventService.GetCurrentUserAsync();
                await LogTransactionEvent("DELETE", user, 0,
                    $"Deleted position: {data.PositionName}",
                    oldData: data, newData: null);

                return Ok(new { Message = $"Position {data.PositionName} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- Transaction Event Helper Methods ---
        private async Task LogTransactionEvent(string action, UserRolesDTOV2 user, int employeeId,
            string description, tblPositions oldData, tblPositions newData)
        {
            string changes = oldData != null && newData != null ? GetChanges(oldData, newData) : "";

            await _transactionEventService.InsertAsync(new TransactionEvent
            {
                Action = action,
                Description = !string.IsNullOrEmpty(changes)
                    ? $"{user.Username} {action}: {changes}"
                    : $"{user.Username} {action}: {description}",
                UserID = user.UserId,
                UserName = user.Username ?? "Unknown",
                Fullname = newData != null
                    ? $"{newData.PositionName}"
                    : oldData != null
                        ? $"{oldData.PositionName}"
                        : "Unknown",
                Timestamp = DateTime.Now
            });
        }

        private string GetChanges(tblPositions oldData, tblPositions newData)
        {
            var changes = new List<string>();
            var properties = typeof(tblPositions).GetProperties();

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldData)?.ToString() ?? "";
                var newValue = prop.GetValue(newData)?.ToString() ?? "";

                if (oldValue != newValue)
                {
                    changes.Add($"{prop.Name}: {oldValue} → {newValue}");
                }
            }

            return changes.Count > 0 ? string.Join(" | ", changes) : "No changes detected";
        }
    }
}