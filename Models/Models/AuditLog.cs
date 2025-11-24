// Models/Models/AuditLog.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogID { get; set; }

        [Required]
        public string TableName { get; set; }

        [Required]
        public string Action { get; set; } // INSERT, UPDATE, DELETE

        public string RecordID { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }

        [Required]
        public int UserID { get; set; }

        public string UserName { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}