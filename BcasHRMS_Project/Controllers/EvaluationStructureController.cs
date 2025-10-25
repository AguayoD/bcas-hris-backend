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

            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                // First check if group has any subgroups
                var checkSubgroupsSql = "SELECT COUNT(1) FROM SubGroup WHERE GroupID = @GroupID";
                var subgroupCount = await _connection.ExecuteScalarAsync<int>(checkSubgroupsSql, new { GroupID = id });

                if (subgroupCount > 0)
                {
                    return BadRequest("Cannot delete group that has subgroups. Please delete subgroups first.");
                }

                var sql = "DELETE FROM [Group] WHERE GroupID = @id";
                var rows = await _connection.ExecuteAsync(sql, new { id });
                return rows > 0 ? Ok(new { message = "Group deleted successfully" }) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting group: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("subgroups/{id}")]
        public async Task<IActionResult> DeleteSubGroup(int id)
        {
            try
            {
                // First check if subgroup has any items
                var checkItemsSql = "SELECT COUNT(1) FROM Item WHERE SubGroupID = @SubGroupID";
                var itemCount = await _connection.ExecuteScalarAsync<int>(checkItemsSql, new { SubGroupID = id });

                if (itemCount > 0)
                {
                    return BadRequest("Cannot delete subgroup that has items. Please delete items first.");
                }

                // Check if subgroup has any evaluation scores
                var checkScoresSql = "SELECT COUNT(1) FROM SubGroupScore WHERE SubGroupID = @SubGroupID";
                var scoreCount = await _connection.ExecuteScalarAsync<int>(checkScoresSql, new { SubGroupID = id });

                if (scoreCount > 0)
                {
                    return BadRequest("Cannot delete subgroup that has evaluation scores. Please delete evaluations first.");
                }

                var sql = "DELETE FROM SubGroup WHERE SubGroupID = @id";
                var rows = await _connection.ExecuteAsync(sql, new { id });
                return rows > 0 ? Ok(new { message = "SubGroup deleted successfully" }) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting subgroup: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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

            return rows > 0 ? Ok() : NotFound();
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                // Check if item has any evaluation scores
                var checkScoresSql = "SELECT COUNT(1) FROM EvaluationScore WHERE ItemID = @ItemID";
                var scoreCount = await _connection.ExecuteScalarAsync<int>(checkScoresSql, new { ItemID = id });

                if (scoreCount > 0)
                {
                    return BadRequest("Cannot delete item that has evaluation scores. Please delete evaluations first.");
                }

                var sql = "DELETE FROM Item WHERE ItemID = @id";
                var rows = await _connection.ExecuteAsync(sql, new { id });
                return rows > 0 ? Ok(new { message = "Item deleted successfully" }) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting item: {ex.Message}");
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