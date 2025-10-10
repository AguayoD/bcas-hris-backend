using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class tblBackupRecords
    {
        [Key]
        public int? BackupID { get; set; }
        public DateTime? BackupDate { get; set; }
        public string? BackupLocation { get; set; }

    }
}
