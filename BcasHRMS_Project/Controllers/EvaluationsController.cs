// Controllers/EvaluationsController.cs
using BCAS_HRMSbackend.Controllers;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Models.Models;
using Repositories.Context;
using System.Data;
using System.Text.Json;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationsController : BaseController
    {
        private readonly IDbConnection _connection;

        public EvaluationsController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
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

            // Validate evaluation date
            if (!DateTime.TryParse(evaluation.EvaluationDate.ToString(), out DateTime evaluationDate))
                return BadRequest("Invalid evaluation date format.");

            if (evaluationDate > DateTime.UtcNow.Date)
                return BadRequest("Evaluation date cannot be in the future.");

            if (evaluationDate.Year < 2000)
                return BadRequest("Invalid evaluation date.");

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

                // NEW STEP: Check if evaluation already exists for this employee and evaluator for the same date
                var checkExistingSql = @"
                    SELECT COUNT(1) FROM Evaluation 
                    WHERE EmployeeID = @EmployeeID AND EvaluatorID = @EvaluatorID AND EvaluationDate = @EvaluationDate";

                var existingCount = await _connection.ExecuteScalarAsync<int>(checkExistingSql, new
                {
                    EmployeeID = evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value,
                    EvaluationDate = evaluationDate.Date
                }, transaction);

                if (existingCount > 0)
                {
                    transaction.Rollback();
                    return BadRequest("This employee has already been evaluated by this evaluator for the selected date.");
                }

                // STEP 2: Calculate Final Score from SubGroupScores with weights
                decimal finalScore = await CalculateWeightedFinalScore(evaluation.Scores, transaction);

                // STEP 3: Insert Evaluation - Use the provided evaluation date
                var insertEvalSql = @"
                    INSERT INTO Evaluation (EmployeeID, EvaluatorID, EvaluationDate, Comments, FinalScore, CreatedAt)
                    VALUES (@EmployeeID, @EvaluatorID, @EvaluationDate, @Comments, @FinalScore, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var evaluationID = await _connection.ExecuteScalarAsync<int>(insertEvalSql, new
                {
                    evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value,
                    EvaluationDate = evaluationDate.Date,
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

                // Log the INSERT action
                await LogActionAsync("Evaluation", "INSERT", evaluationID.ToString(), null, new
                {
                    evaluationID,
                    evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value,
                    evaluation.EvaluationDate,
                    evaluation.Comments,
                    FinalScore = Math.Round(finalScore, 2)
                });

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

        [HttpPost("reset")]
        public async Task<IActionResult> ResetEvaluations()
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                using var transaction = _connection.BeginTransaction();

                try
                {
                    // Instead of deleting, move evaluations to an archive/history table
                    // First check if archive table exists, create it if not
                    var createArchiveTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EvaluationHistory' AND xtype='U')
                        BEGIN
                            CREATE TABLE EvaluationHistory (
                                EvaluationHistoryID int IDENTITY(1,1) PRIMARY KEY,
                                OriginalEvaluationID int,
                                EmployeeID int,
                                EvaluatorID int,
                                EvaluationDate datetime2,
                                Comments nvarchar(MAX),
                                FinalScore float,
                                CreatedAt datetime2,
                                ArchivedAt datetime2 DEFAULT GETDATE(),
                                ScoresJson nvarchar(MAX)
                            )
                        END";

                    await _connection.ExecuteAsync(createArchiveTableSql, transaction: transaction);

                    // Archive current evaluations with their scores as JSON
                    var archiveEvaluationsSql = @"
                        INSERT INTO EvaluationHistory (OriginalEvaluationID, EmployeeID, EvaluatorID, EvaluationDate, Comments, FinalScore, CreatedAt, ScoresJson)
                        SELECT 
                            e.EvaluationID,
                            e.EmployeeID,
                            e.EvaluatorID,
                            e.EvaluationDate,
                            e.Comments,
                            e.FinalScore,
                            e.CreatedAt,
                            (SELECT 
                                SubGroupID, 
                                ScoreValue 
                             FROM SubGroupScore sgs 
                             WHERE sgs.EvaluationID = e.EvaluationID 
                             FOR JSON PATH) as ScoresJson
                        FROM Evaluation e";

                    var archivedCount = await _connection.ExecuteAsync(archiveEvaluationsSql, transaction: transaction);

                    // Now delete the current data (this becomes the "reset")
                    var deleteScoresSql = "DELETE FROM SubGroupScore";
                    var deleteEvalsSql = "DELETE FROM Evaluation";

                    await _connection.ExecuteAsync(deleteScoresSql, transaction: transaction);
                    await _connection.ExecuteAsync(deleteEvalsSql, transaction: transaction);

                    transaction.Commit();

                    // Log the reset action
                    await LogActionAsync("Evaluation", "RESET", "ALL", null, new
                    {
                        Action = "Evaluations archived and current data reset",
                        ArchivedCount = archivedCount
                    });

                    return Ok(new
                    {
                        Message = $"Evaluation data reset successfully. {archivedCount} evaluations moved to history.",
                        ArchivedCount = archivedCount
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error resetting evaluations: {ex.Message}");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetEvaluationHistory()
        {
            try
            {
                // Check if history table exists
                var checkTableSql = @"
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'EvaluationHistory'";

                var tableExists = await _connection.ExecuteScalarAsync<int>(checkTableSql) > 0;

                if (!tableExists)
                {
                    return Ok(new List<EvaluationHistoryDto>()); // Return empty list if no history table
                }

                var sql = @"
                    SELECT 
                        eh.EvaluationHistoryID,
                        eh.OriginalEvaluationID,
                        eh.EmployeeID,
                        emp.FirstName + ' ' + emp.LastName AS EmployeeName,
                        eh.EvaluatorID,
                        evEmp.FirstName + ' ' + evEmp.LastName AS EvaluatorName,
                        eh.EvaluationDate,
                        eh.Comments,
                        eh.FinalScore,
                        eh.CreatedAt,
                        eh.ArchivedAt,
                        eh.ScoresJson
                    FROM EvaluationHistory eh
                    LEFT JOIN tblEmployees emp ON eh.EmployeeID = emp.EmployeeID
                    LEFT JOIN tblUsers u ON eh.EvaluatorID = u.UserId
                    LEFT JOIN tblEmployees evEmp ON u.EmployeeId = evEmp.EmployeeID
                    ORDER BY eh.ArchivedAt DESC, eh.EvaluationDate DESC";

                var result = await _connection.QueryAsync<EvaluationHistoryDto>(sql);

                // Parse the JSON scores for each evaluation
                var evaluationsWithScores = result.Select(eval =>
                {
                    if (!string.IsNullOrEmpty(eval.ScoresJson))
                    {
                        try
                        {
                            var scoresData = JsonSerializer.Deserialize<List<SubGroupScore>>(eval.ScoresJson);
                            eval.Scores = scoresData?.Select(score => new SubGroupAnswer
                            {
                                SubGroupID = score.SubGroupID,
                                ScoreValue = score.ScoreValue,
                                ScoreLabel = GetScoreLabel(score.ScoreValue),
                                SubGroupName = "Archived" // You might want to store this in the JSON too
                            }).ToList() ?? new List<SubGroupAnswer>();
                        }
                        catch (Exception ex)
                        {
                            // If JSON parsing fails, leave scores empty
                            eval.Scores = new List<SubGroupAnswer>();
                        }
                    }
                    else
                    {
                        eval.Scores = new List<SubGroupAnswer>();
                    }
                    return eval;
                }).ToList();

                return Ok(evaluationsWithScores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching evaluation history: {ex.Message}");
            }
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetEvaluationHistoryById(int id)
        {
            try
            {
                var sql = @"
                    SELECT 
                        eh.EvaluationHistoryID,
                        eh.OriginalEvaluationID,
                        eh.EmployeeID,
                        emp.FirstName + ' ' + emp.LastName AS EmployeeName,
                        eh.EvaluatorID,
                        evEmp.FirstName + ' ' + evEmp.LastName AS EvaluatorName,
                        eh.EvaluationDate,
                        eh.Comments,
                        eh.FinalScore,
                        eh.CreatedAt,
                        eh.ArchivedAt,
                        eh.ScoresJson
                    FROM EvaluationHistory eh
                    LEFT JOIN tblEmployees emp ON eh.EmployeeID = emp.EmployeeID
                    LEFT JOIN tblUsers u ON eh.EvaluatorID = u.UserId
                    LEFT JOIN tblEmployees evEmp ON u.EmployeeId = evEmp.EmployeeID
                    WHERE eh.EvaluationHistoryID = @Id";

                var result = await _connection.QueryFirstOrDefaultAsync<EvaluationHistoryDto>(sql, new { Id = id });

                if (result == null)
                    return NotFound("Archived evaluation not found.");

                // Parse the JSON scores
                if (!string.IsNullOrEmpty(result.ScoresJson))
                {
                    try
                    {
                        var scoresData = JsonSerializer.Deserialize<List<SubGroupScore>>(result.ScoresJson);
                        result.Scores = scoresData?.Select(score => new SubGroupAnswer
                        {
                            SubGroupID = score.SubGroupID,
                            ScoreValue = score.ScoreValue,
                            ScoreLabel = GetScoreLabel(score.ScoreValue),
                            SubGroupName = "Archived"
                        }).ToList() ?? new List<SubGroupAnswer>();
                    }
                    catch (Exception ex)
                    {
                        result.Scores = new List<SubGroupAnswer>();
                    }
                }
                else
                {
                    result.Scores = new List<SubGroupAnswer>();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching evaluation history: {ex.Message}");
            }
        }

        private async Task<decimal> CalculateWeightedFinalScore(List<SubGroupScore> scores, IDbTransaction transaction)
        {
            if (scores == null || scores.Count == 0)
                return 0m;

            var groupsSql = "SELECT * FROM [Group]";
            var groups = await _connection.QueryAsync<Group>(groupsSql, transaction: transaction);

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
                if (subgroup.SubGroupID != 0)
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
                ORDER BY e.CreatedAt DESC;";

            var result = await _connection.QueryAsync<EvaluationWithNamesDto>(sql);
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
                WHERE e.EvaluationID = @EvaluationID;";

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
                WHERE sgs.EvaluationID = @EvaluationID;";

            using var multi = await _connection.QueryMultipleAsync(sql, new { EvaluationID = id });

            var evaluation = await multi.ReadFirstOrDefaultAsync<EvaluationWithNamesDto>();
            if (evaluation == null)
                return NotFound();

            var scores = await multi.ReadAsync<SubGroupScore>();

            return Ok(new { Evaluation = evaluation, Scores = scores });
        }

        private string GetScoreLabel(int scoreValue)
        {
            return ScoreChoice.StandardChoices
                .FirstOrDefault(s => s.Value == scoreValue)?.Label ?? "Unknown";
        }
    }
}