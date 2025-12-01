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
            var sql = @"
        SELECT 
            GroupID,
            Name, 
            Description, 
            Weight,
            GroupTypeID 
        FROM [Group]";
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
        public async Task<IActionResult> CreateGroup([FromBody] GroupCreateDto groupDto)
        {
            if (groupDto == null)
                return BadRequest("Group data is required.");

            var sql = @"
                INSERT INTO [Group] (Name, Description, Weight)
                VALUES (@Name, @Description, @Weight);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await _connection.ExecuteScalarAsync<int>(sql, groupDto);
            return Ok(new { GroupID = id });
        }

        [HttpPut("groups/{id}")]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] GroupUpdateDto groupUpdate)
        {
            if (groupUpdate == null)
                return BadRequest("Group data is required.");

            // First get the existing group to preserve other fields
            var existingGroup = await _connection.QueryFirstOrDefaultAsync<Group>(
                "SELECT * FROM [Group] WHERE GroupID = @GroupID",
                new { GroupID = id });

            if (existingGroup == null)
                return NotFound("Group not found.");

            var sql = @"
                UPDATE [Group]
                SET Description = @Description
                WHERE GroupID = @GroupID";

            var rows = await _connection.ExecuteAsync(sql, new
            {
                GroupID = id,
                Description = groupUpdate.Description
            });

            return rows > 0 ? Ok(new { message = "Group updated successfully" }) : NotFound();
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                // First check if group exists
                var checkExistsSql = "SELECT COUNT(1) FROM [Group] WHERE GroupID = @GroupID";
                var existsCount = await _connection.ExecuteScalarAsync<int>(checkExistsSql, new { GroupID = id });

                if (existsCount == 0)
                {
                    return NotFound(new { message = "Group not found." });
                }

                // First check if group has any subgroups
                var checkSubgroupsSql = "SELECT COUNT(1) FROM SubGroup WHERE GroupID = @GroupID";
                var subgroupCount = await _connection.ExecuteScalarAsync<int>(checkSubgroupsSql, new { GroupID = id });

                if (subgroupCount > 0)
                {
                    return BadRequest(new { message = "Cannot delete group that has subgroups. Please delete subgroups first." });
                }

                var sql = "DELETE FROM [Group] WHERE GroupID = @id";
                var rows = await _connection.ExecuteAsync(sql, new { id });

                if (rows > 0)
                {
                    return Ok(new { message = "Group deleted successfully" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to delete group" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting group: {ex.Message}" });
            }
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
        public async Task<IActionResult> CreateSubGroup([FromBody] SubGroupCreateDto subGroupDto)
        {
            if (subGroupDto == null)
                return BadRequest("SubGroup data is required.");

            if (subGroupDto.GroupID == 0)
                return BadRequest("GroupID is required.");

            if (string.IsNullOrEmpty(subGroupDto.Name))
                return BadRequest("SubGroup name is required.");

            try
            {
                var sql = @"
                    INSERT INTO SubGroup (GroupID, Name)
                    VALUES (@GroupID, @Name);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await _connection.ExecuteScalarAsync<int>(sql, new
                {
                    GroupID = subGroupDto.GroupID,
                    Name = subGroupDto.Name
                });

                // Return the complete subgroup with the new ID
                return Ok(new SubGroup
                {
                    SubGroupID = id,
                    GroupID = subGroupDto.GroupID,
                    Name = subGroupDto.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("subgroups/{id}")]
        public async Task<IActionResult> UpdateSubGroup(int id, [FromBody] SubGroupUpdateDto subGroupUpdate)
        {
            if (subGroupUpdate == null)
                return BadRequest("SubGroup data is required.");

            // First get the existing subgroup to preserve other fields
            var existingSubGroup = await _connection.QueryFirstOrDefaultAsync<SubGroup>(
                "SELECT * FROM SubGroup WHERE SubGroupID = @SubGroupID",
                new { SubGroupID = id });

            if (existingSubGroup == null)
                return NotFound("SubGroup not found.");

            var sql = @"
                UPDATE SubGroup
                SET Name = @Name
                WHERE SubGroupID = @SubGroupID";

            var rows = await _connection.ExecuteAsync(sql, new
            {
                SubGroupID = id,
                Name = subGroupUpdate.Name
            });

            return rows > 0 ? Ok(new { message = "SubGroup updated successfully" }) : NotFound();
        }

        [HttpDelete("subgroups/{id}")]
        public async Task<IActionResult> DeleteSubGroup(int id)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using var transaction = _connection.BeginTransaction();

                try
                {
                    // First check if subgroup exists
                    var checkExistsSql = "SELECT COUNT(1) FROM SubGroup WHERE SubGroupID = @SubGroupID";
                    var existsCount = await _connection.ExecuteScalarAsync<int>(checkExistsSql, new { SubGroupID = id }, transaction);

                    if (existsCount == 0)
                    {
                        transaction.Rollback();
                        return NotFound(new { message = "SubGroup not found." });
                    }

                    // First check if subgroup has any items - delete them first
                    var checkItemsSql = "SELECT COUNT(1) FROM Item WHERE SubGroupID = @SubGroupID";
                    var itemCount = await _connection.ExecuteScalarAsync<int>(checkItemsSql, new { SubGroupID = id }, transaction);

                    if (itemCount > 0)
                    {
                        // Delete all items in this subgroup first
                        var deleteItemsSql = "DELETE FROM Item WHERE SubGroupID = @SubGroupID";
                        await _connection.ExecuteAsync(deleteItemsSql, new { SubGroupID = id }, transaction);
                    }

                    // Check if subgroup has any evaluation scores
                    var checkScoresSql = "SELECT COUNT(1) FROM SubGroupScore WHERE SubGroupID = @SubGroupID";
                    var scoreCount = await _connection.ExecuteScalarAsync<int>(checkScoresSql, new { SubGroupID = id }, transaction);

                    if (scoreCount > 0)
                    {
                        // Find evaluations that use this subgroup and delete them
                        var findEvaluationsSql = @"
                            SELECT DISTINCT EvaluationID 
                            FROM SubGroupScore 
                            WHERE SubGroupID = @SubGroupID";

                        var evaluationIds = await _connection.QueryAsync<int>(findEvaluationsSql, new { SubGroupID = id }, transaction);

                        foreach (var evalId in evaluationIds)
                        {
                            // Delete scores for this evaluation
                            var deleteScoresSql = "DELETE FROM SubGroupScore WHERE EvaluationID = @EvaluationID";
                            await _connection.ExecuteAsync(deleteScoresSql, new { EvaluationID = evalId }, transaction);

                            // Delete the evaluation
                            var deleteEvalSql = "DELETE FROM Evaluation WHERE EvaluationID = @EvaluationID";
                            await _connection.ExecuteAsync(deleteEvalSql, new { EvaluationID = evalId }, transaction);
                        }
                    }

                    // Now delete the subgroup
                    var deleteSubGroupSql = "DELETE FROM SubGroup WHERE SubGroupID = @SubGroupID";
                    var rows = await _connection.ExecuteAsync(deleteSubGroupSql, new { SubGroupID = id }, transaction);

                    transaction.Commit();

                    if (rows > 0)
                    {
                        return Ok(new
                        {
                            message = "SubGroup deleted successfully",
                            deletedItems = itemCount,
                            deletedEvaluations = scoreCount > 0 ? "and related evaluations" : ""
                        });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Failed to delete subgroup" });
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting subgroup: {ex.Message}" });
            }
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
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDto itemDto)
        {
            if (itemDto == null)
                return BadRequest("Item data is required.");

            if (string.IsNullOrEmpty(itemDto.Description))
                return BadRequest("Item description is required.");

            if (itemDto.SubGroupID == null || itemDto.SubGroupID == 0)
                return BadRequest("SubGroupID is required.");

            try
            {
                var sql = @"
                    INSERT INTO Item (SubGroupID, Description, ItemType, ItemTypeID)
                    VALUES (@SubGroupID, @Description, @ItemType, @ItemTypeID);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var id = await _connection.ExecuteScalarAsync<int>(sql, new
                {
                    SubGroupID = itemDto.SubGroupID,
                    Description = itemDto.Description,
                    ItemType = itemDto.ItemType,
                    ItemTypeID = itemDto.ItemTypeID
                });

                // Return the complete item with the new ID
                return Ok(new Item
                {
                    ItemID = id,
                    SubGroupID = itemDto.SubGroupID,
                    Description = itemDto.Description,
                    ItemType = itemDto.ItemType,
                    ItemTypeID = itemDto.ItemTypeID
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDto itemUpdate)
        {
            if (itemUpdate == null)
                return BadRequest("Item data is required.");

            // First get the existing item to preserve other fields
            var existingItem = await _connection.QueryFirstOrDefaultAsync<Item>(
                "SELECT * FROM Item WHERE ItemID = @ItemID",
                new { ItemID = id });

            if (existingItem == null)
                return NotFound("Item not found.");

            var sql = @"
                UPDATE Item
                SET Description = @Description,
                    ItemType = @ItemType,
                    ItemTypeID = @ItemTypeID
                WHERE ItemID = @ItemID";

            var rows = await _connection.ExecuteAsync(sql, new
            {
                ItemID = id,
                Description = itemUpdate.Description,
                ItemType = itemUpdate.ItemType ?? existingItem.ItemType,
                ItemTypeID = itemUpdate.ItemTypeID ?? existingItem.ItemTypeID
            });

            return rows > 0 ? Ok(new { message = "Item updated successfully" }) : NotFound();
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using var transaction = _connection.BeginTransaction();

                try
                {
                    // First check if item exists
                    var checkExistsSql = "SELECT COUNT(1) FROM Item WHERE ItemID = @ItemID";
                    var existsCount = await _connection.ExecuteScalarAsync<int>(checkExistsSql, new { ItemID = id }, transaction);

                    if (existsCount == 0)
                    {
                        transaction.Rollback();
                        return NotFound(new { message = "Item not found." });
                    }

                    // Check if item has any evaluation scores in EvaluationScore table
                    var checkEvaluationScoresSql = @"
                        SELECT COUNT(1) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'EvaluationScore'";

                    var evaluationScoreTableExists = await _connection.ExecuteScalarAsync<int>(checkEvaluationScoresSql, transaction: transaction) > 0;

                    if (evaluationScoreTableExists)
                    {
                        var checkScoresSql = "SELECT COUNT(1) FROM EvaluationScore WHERE ItemID = @ItemID";
                        var scoreCount = await _connection.ExecuteScalarAsync<int>(checkScoresSql, new { ItemID = id }, transaction);

                        if (scoreCount > 0)
                        {
                            // Delete evaluation scores for this item
                            var deleteScoresSql = "DELETE FROM EvaluationScore WHERE ItemID = @ItemID";
                            await _connection.ExecuteAsync(deleteScoresSql, new { ItemID = id }, transaction);
                        }
                    }

                    // Now delete the item
                    var deleteItemSql = "DELETE FROM Item WHERE ItemID = @ItemID";
                    var rows = await _connection.ExecuteAsync(deleteItemSql, new { ItemID = id }, transaction);

                    transaction.Commit();

                    if (rows > 0)
                    {
                        return Ok(new { message = "Item deleted successfully" });
                    }
                    else
                    {
                        return StatusCode(500, new { message = "Failed to delete item" });
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting item: {ex.Message}" });
            }
        }
    }

    // DTOs for update operations
    public class GroupUpdateDto
    {
        public string Description { get; set; }
    }

    public class SubGroupUpdateDto
    {
        public string Name { get; set; }
    }

    public class ItemUpdateDto
    {
        public string Description { get; set; }
        public string ItemType { get; set; }
        public int? ItemTypeID { get; set; }
    }

    // DTOs for create operations
    public class GroupCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Weight { get; set; }
    }

    public class SubGroupCreateDto
    {
        public int GroupID { get; set; }
        public string Name { get; set; }
    }

    public class ItemCreateDto
    {
        public int? SubGroupID { get; set; }
        public int? GroupID { get; set; }
        public string Description { get; set; }
        public string ItemType { get; set; }
        public int? ItemTypeID { get; set; }
    }
}