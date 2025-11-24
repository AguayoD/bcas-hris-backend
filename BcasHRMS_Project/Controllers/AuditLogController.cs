// Controllers/AuditLogController.cs
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Service;

namespace BCAS_HRMSbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : BaseController
    {
        public AuditLogController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string tableName = null,
                                                     [FromQuery] string action = null,
                                                     [FromQuery] DateTime? fromDate = null,
                                                     [FromQuery] DateTime? toDate = null,
                                                     [FromQuery] int? userId = null)
        {
            try
            {
                var logs = await _auditLogService.GetAuditLogsAsync(tableName, action, fromDate, toDate, userId);
                return Ok(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving audit logs: {ex.Message}" });
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetAuditLogsPaged([FromQuery] int pageNumber = 1,
                                                          [FromQuery] int pageSize = 50,
                                                          [FromQuery] string tableName = null,
                                                          [FromQuery] string action = null,
                                                          [FromQuery] DateTime? fromDate = null,
                                                          [FromQuery] DateTime? toDate = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                var (logs, totalCount) = await _auditLogService.GetAuditLogsPagedAsync(
                    pageNumber, pageSize, tableName, action, fromDate, toDate);

                return Ok(new
                {
                    success = true,
                    data = logs,
                    pagination = new
                    {
                        pageNumber,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving audit logs: {ex.Message}" });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetAuditSummary([FromQuery] DateTime? fromDate = null,
                                                        [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var sql = @"
                    SELECT 
                        TableName,
                        Action,
                        COUNT(*) as Count
                    FROM AuditLog 
                    WHERE (@FromDate IS NULL OR Timestamp >= @FromDate)
                      AND (@ToDate IS NULL OR Timestamp <= @ToDate)
                    GROUP BY TableName, Action
                    ORDER BY TableName, Action";

                var summary = await _auditLogService._connection.QueryAsync<dynamic>(sql, new { FromDate = fromDate, ToDate = toDate });

                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error retrieving audit summary: {ex.Message}" });
            }
        }
    }
}