using Dapper;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Context;
using System.Data;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationStructureController : ControllerBase
    {
        private readonly IDbConnection _connection;

        public EvaluationStructureController()
        {
            _connection = new ApplicationContext("DefaultSqlConnection").CreateConnection();
        }

        // GROUP ENDPOINTS
        [HttpGet("groups")]
        public async Task<IActionResult> GetAllGroups()
        {
            var sql = "SELECT * FROM [Group]";
            var result = await _connection.QueryAsync<Group>(sql);
            return Ok(result);
        }

        [HttpGet("groups/{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var sql = "SELECT * FROM [Group] WHERE GroupID = @id";
            var result = await _connection.QueryFirstOrDefaultAsync<Group>(sql, new { id });
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] Group group)
        {
            var sql = @"
                INSERT INTO [Group] (Name, Description, Weight)
                VALUES (@Name, @Description, @Weight);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await _connection.ExecuteScalarAsync<int>(sql, group);
            return Ok(new { GroupID = id });
        }

        [HttpPut("groups/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] Group group)
        {
            var sql = @"
                UPDATE [Group]
                SET Name = @Name, Description = @Description, Weight = @Weight
                WHERE GroupID = @GroupID";

            group.GroupID = id;
            var rows = await _connection.ExecuteAsync(sql, group);
            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var sql = "DELETE FROM [Group] WHERE GroupID = @id";
            var rows = await _connection.ExecuteAsync(sql, new { id });
            return rows > 0 ? Ok() : NotFound();
        }

        // SUBGROUP ENDPOINTS
        [HttpGet("subgroups/{groupId}")]
        public async Task<IActionResult> GetSubGroupsByGroupId(int groupId)
        {
            var sql = "SELECT * FROM SubGroup WHERE GroupID = @groupId";
            var result = await _connection.QueryAsync<SubGroup>(sql, new { groupId });
            return Ok(result);
        }

        [HttpGet("subgroups")]
        public async Task<IActionResult> GetAllSubGroups()
        {
            var sql = "SELECT * FROM SubGroup";
            var result = await _connection.QueryAsync<SubGroup>(sql);
            return Ok(result);
        }

        [HttpPost("subgroups")]
        public async Task<IActionResult> CreateSubGroup([FromBody] SubGroup subGroup)
        {
            if (subGroup == null)
                return BadRequest("SubGroup data is required.");

            if (subGroup.GroupID == 0)
                return BadRequest("GroupID is required.");

            if (string.IsNullOrEmpty(subGroup.Name))
                return BadRequest("SubGroup name is required.");

            try
            {
                var sql = @"
            INSERT INTO SubGroup (GroupID, Name)
            VALUES (@GroupID, @Name);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await _connection.ExecuteScalarAsync<int>(sql, new
                {
                    GroupID = subGroup.GroupID,
                    Name = subGroup.Name
                });

                // Return the complete subgroup with the new ID
                return Ok(new SubGroup
                {
                    SubGroupID = id,
                    GroupID = subGroup.GroupID,
                    Name = subGroup.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("subgroups/{id}")]
        public async Task<IActionResult> UpdateSubGroup(int id, [FromBody] SubGroup subGroup)
        {
            var sql = @"
                UPDATE SubGroup
                SET GroupID = @GroupID, Name = @Name
                WHERE SubGroupID = @SubGroupID";

            subGroup.SubGroupID = id;
            var rows = await _connection.ExecuteAsync(sql, subGroup);
            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("subgroups/{id}")]
        public async Task<IActionResult> DeleteSubGroup(int id)
        {
            var sql = "DELETE FROM SubGroup WHERE SubGroupID = @id";
            var rows = await _connection.ExecuteAsync(sql, new { id });
            return rows > 0 ? Ok() : NotFound();
        }

        // ITEM ENDPOINTS
        [HttpGet("items/by-subgroup/{subGroupId}")]
        public async Task<IActionResult> GetItemsBySubGroupId(int subGroupId)
        {
            var sql = "SELECT * FROM Item WHERE SubGroupID = @subGroupId";
            var result = await _connection.QueryAsync<Item>(sql, new { subGroupId });
            return Ok(result);
        }
        [HttpGet("items/by-group/{groupId}")]
        public async Task<IActionResult> GetItemsByGroupId(int groupId)
        {
            var sql = "SELECT * FROM Item WHERE GroupID = @groupId AND SubGroupID IS NULL";
            var result = await _connection.QueryAsync<Item>(sql, new { groupId });
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            if (item == null)
                return BadRequest("Item data is required.");

            if (string.IsNullOrEmpty(item.Description))
                return BadRequest("Item description is required.");

            if (item.SubGroupID == null || item.SubGroupID == 0)
                return BadRequest("SubGroupID is required.");

            try
            {
                var sql = @"
            INSERT INTO Item (SubGroupID, Description)
            VALUES (@SubGroupID, @Description);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await _connection.ExecuteScalarAsync<int>(sql, new
                {
                    SubGroupID = item.SubGroupID,
                    Description = item.Description
                });

                // Return the complete item with the new ID
                return Ok(new Item
                {
                    ItemID = id,
                    SubGroupID = item.SubGroupID,
                    Description = item.Description
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item item)
        {
            var sql = @"
        UPDATE Item
        SET GroupID = @GroupID,
            SubGroupID = @SubGroupID,
            Description = @Description
        WHERE ItemID = @ItemID";

            item.ItemID = id;
            var rows = await _connection.ExecuteAsync(sql, item);
            return rows > 0 ? Ok() : NotFound();
        }


        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var sql = "DELETE FROM Item WHERE ItemID = @id";
            var rows = await _connection.ExecuteAsync(sql, new { id });
            return rows > 0 ? Ok() : NotFound();
        }
    }
}
