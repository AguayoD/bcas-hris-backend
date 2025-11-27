using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace Repositories.Services
{
    public class EmailService
    {
        // You can move these to config or environment variables
        private const string FromName = "BCAS HRIS System";
        private const string FromEmail = "hris@bcas.edu.ph";
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SmtpUser = "deaguayo828@gmail.com";
        private const string SmtpPass = "fuen aiij ottl tsva"; // Better store in config!

        // Alternative credentials
        //private const string SmtpUser = "devillalancechristian1@gmail.com";
        //private const string SmtpPass = "ciko gtyb oodb clpy";

        public async Task SendEmailAsync(string email, string subject, string messageBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(FromName, FromEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = messageBody;
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(SmtpUser, SmtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }

        public async Task SendBonusEligibilityEmailAsync(string employeeEmail, string employeeName, double finalScore, double requiredScore, bool isAssistant)
        {
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
                            <p>Dear {employeeName},</p>
                            <p>We are pleased to inform you that based on your recent performance evaluation, 
                            you have qualified for a performance bonus!</p>
                            
                            <div class='results'>
                                <h3>Your Evaluation Results:</h3>
                                <ul>
                                    <li><strong>Total Average Score:</strong> <span class='score'>{finalScore:F2}</span></li>
                                    <li><strong>Required Score:</strong> {requiredScore}</li>
                                    <li><strong>Position Type:</strong> {(isAssistant ? "Assistant" : "Regular")}</li>
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

            await SendEmailAsync(employeeEmail, subject, messageBody);
        }
    }
}