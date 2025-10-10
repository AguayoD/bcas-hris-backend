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
        // Comments, etc.
    }

}
