using Dapper;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Context;
using System.Data;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationsController : ControllerBase
    {
        private readonly IDbConnection _connection;

        public EvaluationsController()
        {
            _connection = new ApplicationContext("DefaultSqlConnection").CreateConnection();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvaluation([FromBody] Evaluation evaluation)
        {
            if (evaluation == null)
                return BadRequest("Invalid evaluation data.");

            if (evaluation.EmployeeID == 0 || evaluation.EvaluatorID == 0)
                return BadRequest("Missing required evaluation information.");

            if (evaluation.Scores == null || evaluation.Scores.Count == 0)
                return BadRequest("At least one score is required.");

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            using var transaction = _connection.BeginTransaction();

            try
            {
                // STEP 1: Resolve EvaluatorID (UserId) from tblUsers using Evaluator.EmployeeID
                var getUserIdSql = @"SELECT UserId FROM tblUsers WHERE EmployeeId = @EmployeeId";
                var evaluatorUserId = await _connection.ExecuteScalarAsync<int?>(getUserIdSql, new
                {
                    EmployeeId = evaluation.EvaluatorID
                }, transaction);

                if (evaluatorUserId == null)
                {
                    transaction.Rollback();
                    return BadRequest("Evaluator not found in tblUsers.");
                }

                // NEW STEP: Check if evaluation already exists for this employee and evaluator
                var checkExistingSql = @"
                    SELECT COUNT(1) FROM Evaluation 
                    WHERE EmployeeID = @EmployeeID AND EvaluatorID = @EvaluatorID";

                var existingCount = await _connection.ExecuteScalarAsync<int>(checkExistingSql, new
                {
                    EmployeeID = evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value
                }, transaction);

                if (existingCount > 0)
                {
                    transaction.Rollback();
                    return BadRequest("This employee has already been evaluated by this evaluator.");
                }

                // STEP 2: Calculate Final Score from SubGroupScores with weights
                decimal finalScore = await CalculateWeightedFinalScore(evaluation.Scores, transaction);

                // STEP 3: Insert Evaluation
                var insertEvalSql = @"
                    INSERT INTO Evaluation (EmployeeID, EvaluatorID, EvaluationDate, Comments, FinalScore, CreatedAt)
                    VALUES (@EmployeeID, @EvaluatorID, @EvaluationDate, @Comments, @FinalScore, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var evaluationID = await _connection.ExecuteScalarAsync<int>(insertEvalSql, new
                {
                    evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value,
                    EvaluationDate = DateTime.UtcNow,
                    evaluation.Comments,
                    FinalScore = Math.Round(finalScore, 2),
                    CreatedAt = DateTime.UtcNow
                }, transaction);

                // STEP 4: Insert SubGroup Scores
                var insertScoreSql = @"
                    INSERT INTO SubGroupScore (EvaluationID, SubGroupID, ScoreValue)
                    VALUES (@EvaluationID, @SubGroupID, @ScoreValue);";

                foreach (var score in evaluation.Scores)
                {
                    await _connection.ExecuteAsync(insertScoreSql, new
                    {
                        EvaluationID = evaluationID,
                        SubGroupID = score.SubGroupID,
                        ScoreValue = score.ScoreValue
                    }, transaction);
                }

                transaction.Commit();

                // Return the complete evaluation with ID
                evaluation.EvaluationID = evaluationID;
                evaluation.FinalScore = (float)finalScore;
                evaluation.CreatedAt = DateTime.UtcNow;

                return Ok(evaluation);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the final weighted score based on group weights.
        /// </summary>
        private async Task<decimal> CalculateWeightedFinalScore(List<SubGroupScore> scores, IDbTransaction transaction)
        {
            if (scores == null || scores.Count == 0)
                return 0m;

            // Get all groups with their weights
            var groupsSql = "SELECT * FROM [Group]";
            var groups = await _connection.QueryAsync<Group>(groupsSql, transaction: transaction);

            // Get subgroups with their parent groups
            var subgroupsSql = @"
                SELECT sg.SubGroupID, sg.GroupID, sg.Name, g.Weight 
                FROM SubGroup sg 
                INNER JOIN [Group] g ON sg.GroupID = g.GroupID";
            var subgroups = await _connection.QueryAsync<(int SubGroupID, int GroupID, string Name, float Weight)>(subgroupsSql, transaction: transaction);

            decimal totalWeightedScore = 0m;
            decimal totalWeight = 0m;

            foreach (var score in scores)
            {
                var subgroup = subgroups.FirstOrDefault(s => s.SubGroupID == score.SubGroupID);
                if (subgroup.SubGroupID != 0) // Found the subgroup
                {
                    totalWeightedScore += score.ScoreValue * (decimal)subgroup.Weight;
                    totalWeight += (decimal)subgroup.Weight;
                }
            }

            return totalWeight > 0 ? Math.Round(totalWeightedScore / totalWeight, 2) : 0m;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvaluations()
        {
            var sql = @"
                SELECT 
                    e.EvaluationID,
                    e.EmployeeID,
                    emp.FirstName + ' ' + emp.LastName AS EmployeeName,
                    e.EvaluatorID,
                    evEmp.FirstName + ' ' + evEmp.LastName AS EvaluatorName,
                    e.EvaluationDate,
                    e.Comments,
                    e.FinalScore,
                    e.CreatedAt
                FROM Evaluation e
                INNER JOIN tblEmployees emp ON e.EmployeeID = emp.EmployeeID
                INNER JOIN tblUsers u ON e.EvaluatorID = u.UserId
                INNER JOIN tblEmployees evEmp ON u.EmployeeId = evEmp.EmployeeID
                ORDER BY e.CreatedAt DESC;
            ";

            var result = await _connection.QueryAsync<EvaluationWithNamesDto>(sql);

            // Return empty array instead of 404 when no evaluations found
            return Ok(result);
        }

        [HttpGet("{id}/answers")]
        public async Task<IActionResult> GetEvaluationAnswers(int id)
        {
            var sql = @"
                SELECT 
                    sg.ScoreValue,
                    sg.SubGroupID,
                    sgrp.Name AS SubGroupName,
                    u.UserId,
                    evEmp.FirstName + ' ' + evEmp.LastName AS EvaluatorName
                FROM SubGroupScore sg
                INNER JOIN Evaluation e ON sg.EvaluationID = e.EvaluationID
                INNER JOIN tblUsers u ON e.EvaluatorID = u.UserId
                INNER JOIN tblEmployees evEmp ON u.EmployeeId = evEmp.EmployeeID
                INNER JOIN SubGroup sgrp ON sg.SubGroupID = sgrp.SubGroupID
                WHERE e.EvaluationID = @EvaluationID;
            ";

            var result = await _connection.QueryAsync<dynamic>(sql, new { EvaluationID = id });

            if (!result.Any())
                return NotFound("Evaluation not found or no answers.");

            var first = result.First();
            var dto = new EvaluationAnswerDto
            {
                EvaluatorName = first.EvaluatorName,
                Answers = result.Select(r => new SubGroupAnswer
                {
                    SubGroupID = r.SubGroupID,
                    SubGroupName = r.SubGroupName,
                    ScoreValue = r.ScoreValue,
                    ScoreLabel = GetScoreLabel((int)r.ScoreValue)
                }).ToList()
            };

            return Ok(dto);
        }

        private string GetScoreLabel(int scoreValue)
        {
            return ScoreChoice.StandardChoices
                .FirstOrDefault(s => s.Value == scoreValue)?.Label ?? "Unknown";
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetEvaluations()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                var deleteScoresSql = "DELETE FROM SubGroupScore";
                var deleteEvalsSql = "DELETE FROM Evaluation";

                await _connection.ExecuteAsync(deleteScoresSql);
                await _connection.ExecuteAsync(deleteEvalsSql);

                return Ok(new { Message = "Evaluation data reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error resetting evaluations: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvaluationById(int id)
        {
            var sql = @"
                SELECT 
                    e.*,
                    emp.FirstName + ' ' + emp.LastName AS EmployeeName,
                    evEmp.FirstName + ' ' + evEmp.LastName AS EvaluatorName
                FROM Evaluation e
                INNER JOIN tblEmployees emp ON e.EmployeeID = emp.EmployeeID
                INNER JOIN tblUsers u ON e.EvaluatorID = u.UserId
                INNER JOIN tblEmployees evEmp ON u.EmployeeId = evEmp.EmployeeID
                WHERE e.EvaluationID = @EvaluationID;

                SELECT 
                    sgs.*,
                    sg.Name AS SubGroupName
                FROM SubGroupScore sgs
                INNER JOIN SubGroup sg ON sgs.SubGroupID = sg.SubGroupID
                WHERE sgs.EvaluationID = @EvaluationID;
            ";

            using var multi = await _connection.QueryMultipleAsync(sql, new { EvaluationID = id });

            var evaluation = await multi.ReadFirstOrDefaultAsync<EvaluationWithNamesDto>();
            if (evaluation == null)
                return NotFound();

            var scores = await multi.ReadAsync<SubGroupScore>();

            return Ok(new { Evaluation = evaluation, Scores = scores });
        }
    }
}