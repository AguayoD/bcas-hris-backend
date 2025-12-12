// [file name]: tblPendingEmployeeUpdateRepository.cs
using Models.Models;
using Repositories.Context;
using System.Data;
using Dapper;
using Models.Models;
using Repositories.Context;
using System.Data;

namespace Repositories.Repositories
{
    public class tblPendingEmployeeUpdateRepository
    {
        private readonly IDbConnection _connection;

        public tblPendingEmployeeUpdateRepository(string connectionString = "DefaultSqlConnection")
        {
            _connection = new ApplicationContext(connectionString).CreateConnection();
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetAllAsync()
        {
            string sql = @"
                SELECT p.*, e.* 
                FROM tblPendingEmployeeUpdates p
                LEFT JOIN tblEmployees e ON p.EmployeeID = e.EmployeeID
                ORDER BY p.SubmittedAt DESC";

            return await _connection.QueryAsync<tblPendingEmployeeUpdate, tblEmployees, tblPendingEmployeeUpdate>(
                sql,
                (pendingUpdate, employee) =>
                {
                    pendingUpdate.Employee = employee;
                    return pendingUpdate;
                },
                splitOn: "EmployeeID"
            );
        }

        public async Task<tblPendingEmployeeUpdate> GetByIdAsync(int id)
        {
            string sql = @"
                SELECT p.*, e.* 
                FROM tblPendingEmployeeUpdates p
                LEFT JOIN tblEmployees e ON p.EmployeeID = e.EmployeeID
                WHERE p.PendingUpdateID = @Id";

            var result = await _connection.QueryAsync<tblPendingEmployeeUpdate, tblEmployees, tblPendingEmployeeUpdate>(
                sql,
                (pendingUpdate, employee) =>
                {
                    pendingUpdate.Employee = employee;
                    return pendingUpdate;
                },
                new { Id = id },
                splitOn: "EmployeeID"
            );

            return result.FirstOrDefault();
        }

        public async Task<tblPendingEmployeeUpdate> InsertAsync(tblPendingEmployeeUpdate pendingUpdate)
        {
            string sql = @"
                INSERT INTO tblPendingEmployeeUpdates 
                (EmployeeID, UpdateData, OriginalData, Status, SubmittedAt, ReviewedAt, ReviewedBy, ReviewerName, Comments)
                VALUES (@EmployeeID, @UpdateData, @OriginalData, @Status, @SubmittedAt, @ReviewedAt, @ReviewedBy, @ReviewerName, @Comments);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await _connection.ExecuteScalarAsync<int>(sql, pendingUpdate);
            pendingUpdate.PendingUpdateID = id;
            return pendingUpdate;
        }

        public async Task<tblPendingEmployeeUpdate> UpdateAsync(tblPendingEmployeeUpdate pendingUpdate)
        {
            string sql = @"
                UPDATE tblPendingEmployeeUpdates 
                SET Status = @Status,
                    ReviewedAt = @ReviewedAt,
                    ReviewedBy = @ReviewedBy,
                    ReviewerName = @ReviewerName,
                    Comments = @Comments
                WHERE PendingUpdateID = @PendingUpdateID";

            await _connection.ExecuteAsync(sql, pendingUpdate);
            return pendingUpdate;
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetByStatusAsync(string status)
        {
            string sql = @"
                SELECT p.*, e.* 
                FROM tblPendingEmployeeUpdates p
                LEFT JOIN tblEmployees e ON p.EmployeeID = e.EmployeeID
                WHERE p.Status = @Status
                ORDER BY p.SubmittedAt DESC";

            return await _connection.QueryAsync<tblPendingEmployeeUpdate, tblEmployees, tblPendingEmployeeUpdate>(
                sql,
                (pendingUpdate, employee) =>
                {
                    pendingUpdate.Employee = employee;
                    return pendingUpdate;
                },
                new { Status = status },
                splitOn: "EmployeeID"
            );
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetByEmployeeIdAsync(int employeeId)
        {
            string sql = @"
                SELECT p.*, e.* 
                FROM tblPendingEmployeeUpdates p
                LEFT JOIN tblEmployees e ON p.EmployeeID = e.EmployeeID
                WHERE p.EmployeeID = @EmployeeId
                ORDER BY p.SubmittedAt DESC";

            return await _connection.QueryAsync<tblPendingEmployeeUpdate, tblEmployees, tblPendingEmployeeUpdate>(
                sql,
                (pendingUpdate, employee) =>
                {
                    pendingUpdate.Employee = employee;
                    return pendingUpdate;
                },
                new { EmployeeId = employeeId },
                splitOn: "EmployeeID"
            );
        }

        public async Task DeleteOldUpdatesAsync(int daysToKeep = 30)
        {
            string sql = @"
                DELETE FROM tblPendingEmployeeUpdates 
                WHERE SubmittedAt < DATEADD(day, -@DaysToKeep, GETDATE()) 
                AND Status != 'pending'";

            await _connection.ExecuteAsync(sql, new { DaysToKeep = daysToKeep });
        }
    }
}
