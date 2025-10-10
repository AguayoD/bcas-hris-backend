using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblContracts
    {
        [Key]
        public int? ContractID { get; set; }
        public int? EmployeeID { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string? ContractType { get; set; }
        public string? ContractStatus { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}