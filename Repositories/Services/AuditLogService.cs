// Repositories/Service/AuditLogService.cs
using Dapper;
using Models.Models;
using Repositories.Context;
using System.Data;
using System.Text.Json;

namespace Repositories.Service
{
    public class AuditLogService
    {
        public readonly IDbConnection _connection;

        public AuditLogService()
        {
            _connection = new ApplicationContext("DefaultSqlConnection").CreateConnection();
        }

        public async Task LogActionAsync(string tableName, string action, string recordId,
                                       object oldValues, object newValues, int userId,
                                       string userName, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var sql = @"
                    INSERT INTO AuditLog (TableName, Action, RecordID, OldValues, NewValues, 
                                        UserID, UserName, Timestamp, IPAddress, UserAgent)
                    VALUES (@TableName, @Action, @RecordID, @OldValues, @NewValues, 
                            @UserID, @UserName, @Timestamp, @IPAddress, @UserAgent)";

                var auditLog = new AuditLog
                {
                    TableName = tableName,
                    Action = action,
                    RecordID = recordId,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = false }) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = false }) : null,
                    UserID = userId,
                    UserName = userName,
                    Timestamp = DateTime.UtcNow,
                    IPAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _connection.ExecuteAsync(sql, auditLog);
            }
            catch (Exception ex)
            {
                // Fallback logging
                Console.WriteLine($"Audit log failed - Table: {tableName}, Action: {action}, Error: {ex.Message}");
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string tableName = null,
                                                                  string action = null,
                                                                  DateTime? fromDate = null,
                                                                  DateTime? toDate = null,
                                                                  int? userId = null)
        {
            var sql = @"SELECT * FROM AuditLog WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(tableName))
            {
                sql += " AND TableName = @TableName";
                parameters.Add("TableName", tableName);
            }

            if (!string.IsNullOrEmpty(action))
            {
                sql += " AND Action = @Action";
                parameters.Add("Action", action);
            }

            if (fromDate.HasValue)
            {
                sql += " AND Timestamp >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                sql += " AND Timestamp <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            if (userId.HasValue)
            {
                sql += " AND UserID = @UserID";
                parameters.Add("UserID", userId.Value);
            }

            sql += " ORDER BY Timestamp DESC";

            return await _connection.QueryAsync<AuditLog>(sql, parameters);
        }

        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetAuditLogsPagedAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string tableName = null,
            string action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var offset = (pageNumber - 1) * pageSize;

            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(tableName))
            {
                whereClause += " AND TableName = @TableName";
                parameters.Add("TableName", tableName);
            }

            if (!string.IsNullOrEmpty(action))
            {
                whereClause += " AND Action = @Action";
                parameters.Add("Action", action);
            }

            if (fromDate.HasValue)
            {
                whereClause += " AND Timestamp >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                whereClause += " AND Timestamp <= @ToDate";
                parameters.Add("ToDate", toDate.Value);
            }

            var sql = $@"
                SELECT * FROM AuditLog 
                {whereClause}
                ORDER BY Timestamp DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var countSql = $"SELECT COUNT(*) FROM AuditLog {whereClause}";

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            var logs = await _connection.QueryAsync<AuditLog>(sql, parameters);
            var totalCount = await _connection.ExecuteScalarAsync<int>(countSql, parameters);

            return (logs, totalCount);
        }
    }
}