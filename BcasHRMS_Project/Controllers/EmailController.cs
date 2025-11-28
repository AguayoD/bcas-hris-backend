using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using Repositories.Context;
using Repositories.Services;
using System.Text.Json;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IDbConnection _connection;
        private readonly EmailService _emailService;

        public EmailController()
        {
            _connection = new ApplicationContext("DefaultSqlConnection").CreateConnection();
            _emailService = new EmailService();
        }

        [HttpPost("send-bonus-eligibility")]
        public async Task<IActionResult> SendBonusEligibilityEmails([FromBody] BonusEligibilityRequest request)
        {
            if (request?.EligibleEmployees == null || !request.EligibleEmployees.Any())
                return BadRequest("No eligible employees provided");

            var results = new List<EmailResult>();
            int successCount = 0;
            int failCount = 0;

            foreach (var employee in request.EligibleEmployees)
            {
                try
                {
                    // Get employee email from database
                    var emailSql = @"
                        SELECT Email 
                        FROM tblEmployees 
                        WHERE EmployeeID = @EmployeeID";

                    var employeeEmail = await _connection.QueryFirstOrDefaultAsync<string>(
                        emailSql,
                        new { EmployeeID = employee.Id }
                    );

                    if (string.IsNullOrEmpty(employeeEmail))
                    {
                        results.Add(new EmailResult
                        {
                            EmployeeName = employee.Name,
                            Success = false,
                            Message = "No email found"
                        });
                        failCount++;
                        continue;
                    }

                    // Create email content
                    var subject = "Performance Bonus Eligibility Notification";
                    var messageBody = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
                                .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
                                .results {{ background: white; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
                                .requirements {{ background: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
                                .footer {{ margin-top: 20px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; }}
                                .score {{ font-size: 18px; font-weight: bold; color: #28a745; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>Performance Bonus Eligibility</h1>
                                </div>
                                <div class='content'>
                                    <p>Dear {employee.Name},</p>
                                    <p>We are pleased to inform you that based on your recent performance evaluation, 
                                    you have qualified for a performance bonus!</p>
                                    
                                    <div class='results'>
                                        <h3>Your Evaluation Results:</h3>
                                        <ul>
                                            <li><strong>Total Average Score:</strong> <span class='score'>{employee.FinalScore:F2}</span></li>
                                            <li><strong>Required Score:</strong> {employee.RequiredScore}</li>
                                            <li><strong>Position Type:</strong> {(employee.IsAssistant ? "Assistant" : "Regular")}</li>
                                            <li><strong>Status:</strong> <strong style='color: #28a745;'>ELIGIBLE</strong></li>
                                        </ul>
                                    </div>

                                    <div class='requirements'>
                                        <h3>Bonus Requirements:</h3>
                                        <p>To receive this bonus, please ensure you meet the following requirements:</p>
                                        <ul>
                                            <li>No more than 5 late arrivals in the current school year</li>
                                            <li>No more than 3 absences in the current school year</li>
                                        </ul>
                                    </div>

                                    <p>Please contact the HR department if you have any questions or need to verify your attendance records.</p>
                                    
                                    <div class='footer'>
                                        <p>Best regards,<br/>
                                        <strong>BCAS HR Department</strong><br/>
                                        BCAS HRIS System</p>
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>";

                    // Send email
                    await _emailService.SendEmailAsync(employeeEmail, subject, messageBody);

                    results.Add(new EmailResult
                    {
                        EmployeeName = employee.Name,
                        Success = true,
                        Message = "Email sent successfully"
                    });
                    successCount++;
                }
                catch (Exception ex)
                {
                    results.Add(new EmailResult
                    {
                        EmployeeName = employee.Name,
                        Success = false,
                        Message = $"Failed to send email: {ex.Message}"
                    });
                    failCount++;
                }
            }

            return Ok(new
            {
                Success = true,
                Message = $"Emails sent: {successCount} successful, {failCount} failed",
                Results = results
            });
        }
    }

    public class BonusEligibilityRequest
    {
        public List<EligibleEmployeeDto> EligibleEmployees { get; set; } = new List<EligibleEmployeeDto>();
    }

    public class EligibleEmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double FinalScore { get; set; }
        public double RequiredScore { get; set; }
        public bool IsAssistant { get; set; }
    }

    public class EmailResult
    {
        public string EmployeeName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}