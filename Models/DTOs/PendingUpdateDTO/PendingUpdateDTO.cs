
using System;
using System.Collections.Generic;

namespace Models.DTOs
{
    public class SubmitUpdateRequestDTO
    {
        public int EmployeeId { get; set; }
        public Dictionary<string, object> UpdateData { get; set; } = new Dictionary<string, object>();
    }

    public class ReviewUpdateRequestDTO
    {
        public string? Comments { get; set; }
    }

    public class PendingUpdateResponseDTO
    {
        public int PendingUpdateID { get; set; }
        public int EmployeeID { get; set; }
        public Dictionary<string, object> UpdateData { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> OriginalData { get; set; } = new Dictionary<string, object>();
        public string Status { get; set; } = "pending";
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public string? Comments { get; set; }
        public EmployeeBasicDTO? Employee { get; set; }
    }

    public class EmployeeBasicDTO
    {
        public int EmployeeID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? PositionID { get; set; }
        public int? DepartmentID { get; set; }
    }
}
