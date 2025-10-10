using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? DocumentType { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public byte[]? Data { get; set; }

    }

}
