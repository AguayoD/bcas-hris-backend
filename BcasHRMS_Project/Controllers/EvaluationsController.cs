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

            if (evaluation.EmployeeID == 0 || evaluation.EvaluatorID == 0 || evaluation.Scores == null || evaluation.Scores.Count == 0)
                return BadRequest("Missing required evaluation information.");

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
                    EmployeeId = evaluation.EvaluatorID // From localStorage
                }, transaction);

                if (evaluatorUserId == null)
                {
                    transaction.Rollback();
                    return BadRequest("Evaluator not found in tblUsers.");
                }

                // STEP 2: Optional: Calculate Final Score from SubGroupScores
                float finalScore = CalculateFinalScore(evaluation.Scores);

                // STEP 3: Insert Evaluation
                var insertEvalSql = @"
                    INSERT INTO Evaluation (EmployeeID, EvaluatorID, EvaluationDate, Comments, FinalScore, CreatedAt)
                    VALUES (@EmployeeID, @EvaluatorID, @EvaluationDate, @Comments, @FinalScore, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var evaluationID = await _connection.ExecuteScalarAsync<int>(insertEvalSql, new
                {
                    evaluation.EmployeeID,
                    EvaluatorID = evaluatorUserId.Value,
                    evaluation.EvaluationDate,
                    evaluation.Comments,
                    FinalScore = finalScore,
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

                return Ok(new { EvaluationID = evaluationID, Message = "Evaluation created successfully." });
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
        private float CalculateFinalScore(List<SubGroupScore> scores)
        {
            if (scores == null || scores.Count == 0)
                return 0f;

            return (float)scores.Average(s => s.ScoreValue);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvaluation()
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


    }

}
