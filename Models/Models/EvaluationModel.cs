using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Evaluation
    {
        public int EvaluationID { get; set; }
        public int EmployeeID { get; set; }
        public int EvaluatorID { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string Comments { get; set; }
        public float FinalScore { get; set; }
        public DateTime CreatedAt { get; set; }

        // Now, we're scoring SubGroups directly
        public List<SubGroupScore> Scores { get; set; }
    }

    public class SubGroupScore
    {
        public int EvaluationID { get; set; }
        public int SubGroupID { get; set; }
        public int ScoreValue { get; set; }  // 1–5, from ScoreChoice
    }

    public class Group
    {
        public int GroupID { get; set; }
        public string Name { get; set; }          // e.g., "A"
        public string Description { get; set; }   // e.g., "Teaching Profession Qualifications"
        public float Weight { get; set; }          // e.g., 25.0
        public List<SubGroup> SubGroups { get; set; }
    }

    public class SubGroup
    {
        public int SubGroupID { get; set; }
        public int GroupID { get; set; }
        public string Name { get; set; }          // e.g., "Mastery of the Subject Matter"
        public List<Item> Items { get; set; }
        public List<ScoreChoice> ScoreChoices => ScoreChoice.StandardChoices;
    }

    public class Item
    {
        public int ItemID { get; set; }
        public int? SubGroupID { get; set; } // Optional
        [JsonIgnore]
        public int? GroupID { get; set; }
        public string Description { get; set; }  // e.g., "Gives knowledgeable answers to students' questions"
        public string ItemType { get; set; }     // "teaching" or "non-teaching"
        public int? ItemTypeID { get; set; }     // 1 for teaching, 2 for non-teaching
    }

    public class EvaluationScore
    {
        public int EvaluationScoreID { get; set; }
        public int EvaluationID { get; set; }
        public int ItemID { get; set; }
        public int Score { get; set; }
    }

    public class ScoreChoice
    {
        public int Value { get; set; }         // e.g., 1
        public string Label { get; set; }      // e.g., "Poor"

        // Static standard choices
        public static List<ScoreChoice> StandardChoices => new List<ScoreChoice>
        {
            new ScoreChoice { Value = 1, Label = "Poor" },
            new ScoreChoice { Value = 2, Label = "Fair" },
            new ScoreChoice { Value = 3, Label = "Satisfactory" },
            new ScoreChoice { Value = 4, Label = "Very Satisfactory" },
            new ScoreChoice { Value = 5, Label = "Excellent" }
        };
    }

    public class EvaluationWithNamesDto
    {
        public int EvaluationID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public int EvaluatorID { get; set; }
        public string EvaluatorName { get; set; }
        public DateTime EvaluationDate { get; set; }
        public float FinalScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comments { get; set; }
    }

    public class EvaluationAnswerDto
    {
        public string EvaluatorName { get; set; }
        public List<SubGroupAnswer> Answers { get; set; }
    }

    public class SubGroupAnswer
    {
        public int SubGroupID { get; set; }
        public string SubGroupName { get; set; }
        public int ScoreValue { get; set; }
        public string ScoreLabel { get; set; }
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

    // DTOs for responses
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    // Additional models for extended functionality
    public class EvaluationSummary
    {
        public int EvaluationID { get; set; }
        public string EmployeeName { get; set; }
        public string EvaluatorName { get; set; }
        public DateTime EvaluationDate { get; set; }
        public float FinalScore { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
    }

    public class EvaluationDetail
    {
        public int EvaluationID { get; set; }
        public Employee Employee { get; set; }
        public Employee Evaluator { get; set; }
        public DateTime EvaluationDate { get; set; }
        public string Comments { get; set; }
        public float FinalScore { get; set; }
        public List<SubGroupScoreDetail> Scores { get; set; }
    }

    public class SubGroupScoreDetail
    {
        public int SubGroupID { get; set; }
        public string SubGroupName { get; set; }
        public int ScoreValue { get; set; }
        public string ScoreLabel { get; set; }
        public List<ItemScore> ItemScores { get; set; }
    }

    public class ItemScore
    {
        public int ItemID { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
    }

    // Employee model for evaluation context
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
        public int? DepartmentID2 { get; set; }
        public int? DepartmentID3 { get; set; }
    }

    // Department model
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public int? ParentDepartmentID { get; set; }
    }

    // User model for authentication context
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Role model
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
    }

    // Statistics models
    public class EvaluationStats
    {
        public int TotalEvaluations { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalEvaluators { get; set; }
        public decimal AverageScore { get; set; }
        public List<DepartmentStats> DepartmentStats { get; set; }
    }

    public class DepartmentStats
    {
        public string DepartmentName { get; set; }
        public int EvaluationCount { get; set; }
        public decimal AverageScore { get; set; }
    }

    // Report models
    public class EvaluationReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<EvaluationSummary> Evaluations { get; set; }
        public EvaluationStats Statistics { get; set; }
    }

    // Search and filter models
    public class EvaluationFilter
    {
        public int? DepartmentID { get; set; }
        public int? EmployeeID { get; set; }
        public int? EvaluatorID { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // Pagination model
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}