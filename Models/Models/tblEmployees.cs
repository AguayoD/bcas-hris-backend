
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class tblEmployees
    {
        [Key]
        public int? EmployeeID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public int? PositionID { get; set; }
        public int? DepartmentID { get; set; }
        public int? EmploymentStatus { get; set; }
        public DateTime? HireDate { get; set; }
        public string? MemberFirstName { get; set; }
        public string? MemberLastName{ get; set; }
        public string? MemberGender { get; set; }
        public string? MemberAddress { get; set; }
        public string? MemberPhoneNumber { get; set; }
        public int? EducationalAttainment { get; set; }
        public string? InstitutionName { get; set; }
        public DateTime? YearGraduated { get; set; }
        public string? CourseName { get; set; }
        public string? PreviousPosition { get; set; }
        public string? OfficeName { get; set; }
        public DateTime? DurationStart { get; set; }
        public DateTime? DurationEnd { get; set; }
        public string? AgencyName { get; set; }
        public string? Supervisor { get; set; }
        public string? Accomplishment { get; set; }
        public string? Summary{ get; set; }
        //added
        //public List<Evaluation> Evaluations { get; set; }
    }
}
