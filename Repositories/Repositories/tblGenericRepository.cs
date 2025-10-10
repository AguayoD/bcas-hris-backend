using Dapper;
using Models.Enums;
using Models.Models;
using Repositories.Context;
using System.Data;

namespace Repositories.Repositories
{
    public class tblGenericRepository<T> where T : class
    {
        public IDbConnection _connection;
        public int _commandTimeout;
        public string tableName;

        public tblGenericRepository(string connectionString = "DefaultSqlConnection")
        {
            _connection = new ApplicationContext(connectionString).CreateConnection();
            _commandTimeout = 120;
            tableName = typeof(T).Name;
        }

        //ADDED
        // Insert for FileModel
        public async Task<int> InsertFileAsync(FileModel file)
        {
            string sql = $@"
        INSERT INTO {tableName} (EmployeeId, DocumentType, FileName, ContentType, Data)
        VALUES (@EmployeeId, @DocumentType, @FileName, @ContentType, @Data);
        SELECT CAST(SCOPE_IDENTITY() as int);";

            return await _connection.ExecuteScalarAsync<int>(sql, file);
        }


        public async Task<List<FileModel>> GetFilesByEmployeeAsync(int employeeId)
        {
            string sql = $"SELECT * FROM {tableName} WHERE EmployeeId = @EmployeeId";
            var result = await _connection.QueryAsync<FileModel>(sql, new { EmployeeId = employeeId });
            return result.ToList();
        }
        public async Task<FileModel?> GetFileByEmployeeAndDocumentTypeAsync(int employeeId, string documentType)
        {
            string sql = $@"
        SELECT * FROM {tableName}
        WHERE EmployeeId = @EmployeeId AND DocumentType = @DocumentType";

            return await _connection.QueryFirstOrDefaultAsync<FileModel>(
                sql,
                new { EmployeeId = employeeId, DocumentType = documentType }
            );
        }

        public async Task<bool> UpdateFileAsync(FileModel file)
        {
            string sql = $@"
        UPDATE {tableName}
        SET FileName = @FileName,
            ContentType = @ContentType,
            Data = @Data
        WHERE Id = @Id";

            int rowsAffected = await _connection.ExecuteAsync(sql, file);
            return rowsAffected > 0;
        }


        // Get by ID
        public async Task<FileModel> GetFileByIdAsync(int id)
        {
            string sql = $"SELECT * FROM {tableName} WHERE Id = @Id";
            return await _connection.QueryFirstOrDefaultAsync<FileModel>(sql, new { Id = id });
        }

        private string ProcedureName(ProcedureTypes procedureType)
        {
            return $"{tableName}_{procedureType.ToString()}";
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            var procedureName = ProcedureName(ProcedureTypes.GetAll);
            var result = await _connection.QueryAsync<T>(procedureName, commandTimeout: _commandTimeout,
            commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public virtual async Task<T?> GetById(int id)
        {
            var procedureName = ProcedureName(ProcedureTypes.GetById);
            return await _connection.QueryFirstOrDefaultAsync<T>
                  (procedureName.ToString(), new { Id = id }, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }
        public virtual async Task<T?> Insert(T parameters)
        {
            var procedureName = ProcedureName(ProcedureTypes.Insert);
            return await _connection.QueryFirstOrDefaultAsync<T>
                  (procedureName.ToString(), parameters, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }
        public virtual async Task<IEnumerable<T>> InsertMany(IEnumerable<T> parameters)
        {
            List<T> results = new List<T>();
            foreach (var parameter in parameters)
            {
                var newData = await Insert(parameter);
                results.Add(newData);
            }
            return results;
        }
        public virtual async Task<T?> Update(T parameters)
        {
            var procedureName = ProcedureName(ProcedureTypes.Update);
            return await _connection.QueryFirstOrDefaultAsync<T>
                  (procedureName.ToString(), parameters, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
        }
        public virtual async Task<T?> DeleteById(int id)
        {
            var deletedData = await GetById(id);
            var procedureName = ProcedureName(ProcedureTypes.DeleteById);
            _connection.Execute(procedureName.ToString(), new { Id = id }, commandType: CommandType.StoredProcedure, commandTimeout: _commandTimeout);
            return deletedData;
        }
        public virtual async Task<T?> InsertOrUpdate(int? id, T data)
        {
            if (id == null || id == 0) return await Insert(data);
            return await Update(data);
        }
        public async Task<DataTable> InsertManyDT(DataTable dt)
        {
            var procedureName = ProcedureName(ProcedureTypes.InsertMany);
            await _connection.ExecuteAsync(procedureName.ToString(), new { TVP = dt.AsTableValuedParameter($"TVP_{tableName}") }, commandType: CommandType.StoredProcedure);
            return dt;
        }
        public async Task<DataTable> UpdateManyDT(DataTable dt)
        {
            var procedureName = ProcedureName(ProcedureTypes.UpdateMany);
            await _connection.ExecuteAsync(procedureName.ToString(), new { TVP = dt.AsTableValuedParameter($"TVP_{tableName}") }, commandType: CommandType.StoredProcedure);
            return dt;
        }


    }
}
