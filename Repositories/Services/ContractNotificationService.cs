// ContractNotificationService.cs - DEBUG VERSION
using Model.Models;
using Models.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Services
{
    public class ContractNotificationService
    {
        private readonly tblContractsRepository _contractsRepository = new tblContractsRepository();
        private readonly tblEmployeesRepository _employeesRepository = new tblEmployeesRepository();
        private readonly EmailService _emailService = new EmailService();

        public async Task CheckAndSendContractNotifications()
        {
            try
            {
                var allContracts = await _contractsRepository.GetAll();
                var today = DateTime.Today;
                int emailsSent = 0;

                Console.WriteLine($"=== CONTRACT NOTIFICATION CHECK STARTED ===");
                Console.WriteLine($"Today's date: {today:yyyy-MM-dd}");
                Console.WriteLine($"Total contracts to check: {allContracts.Count()}");

                foreach (var contract in allContracts)
                {
                    try
                    {
                        Console.WriteLine($"--- Checking Contract ID: {contract.ContractID} ---");

                        // Skip regular contracts and contracts without end dates
                        if (contract.ContractType == "Regular")
                        {
                            Console.WriteLine($"Skipped - Regular contract");
                            continue;
                        }

                        if (!contract.ContractEndDate.HasValue)
                        {
                            Console.WriteLine($"Skipped - No end date");
                            continue;
                        }

                        var employee = await _employeesRepository.GetById(contract.EmployeeID.Value);
                        if (employee == null)
                        {
                            Console.WriteLine($"Skipped - Employee not found (ID: {contract.EmployeeID})");
                            continue;
                        }

                        if (string.IsNullOrEmpty(employee.Email))
                        {
                            Console.WriteLine($"Skipped - No email for employee {employee.FirstName} {employee.LastName}");
                            continue;
                        }

                        var endDate = contract.ContractEndDate.Value.Date;
                        var daysUntilExpiry = (endDate - today).Days;

                        Console.WriteLine($"Contract: {contract.ContractType}");
                        Console.WriteLine($"Employee: {employee.FirstName} {employee.LastName}");
                        Console.WriteLine($"Email: {employee.Email}");
                        Console.WriteLine($"End Date: {endDate:yyyy-MM-dd}");
                        Console.WriteLine($"Days Until Expiry: {daysUntilExpiry}");

                        // Check notification conditions
                        if (daysUntilExpiry == 31)
                        {
                            Console.WriteLine($"🎯 MATCH - 30 days remaining - SENDING EMAIL");
                            await SendNotification(employee, contract, "1 month", 31);
                            emailsSent++;
                        }
                        else if (daysUntilExpiry == 14)
                        {
                            Console.WriteLine($"🎯 MATCH - 14 days remaining - SENDING EMAIL");
                            await SendNotification(employee, contract, "2 weeks", 14);
                            emailsSent++;
                        }
                        else if (daysUntilExpiry == 7)
                        {
                            Console.WriteLine($"🎯 MATCH - 7 days remaining - SENDING EMAIL");
                            await SendNotification(employee, contract, "1 week", 7);
                            emailsSent++;
                        }
                        else if (daysUntilExpiry <= 0)
                        {
                            Console.WriteLine($"🎯 MATCH - EXPIRED ({daysUntilExpiry} days) - SENDING EMAIL");
                            await SendExpiredNotification(employee, contract);
                            emailsSent++;
                        }
                        else
                        {
                            Console.WriteLine($"No match - {daysUntilExpiry} days remaining (not 31, 14, 7, or expired)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ ERROR processing contract {contract.ContractID}: {ex.Message}");
                    }
                }

                Console.WriteLine($"=== CONTRACT NOTIFICATION CHECK COMPLETED ===");
                Console.WriteLine($"Total emails sent: {emailsSent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FATAL ERROR in notification service: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task SendNotification(tblEmployees employee, tblContracts contract, string timeframe, int days)
        {
            try
            {
                var subject = $"Contract Expires in {timeframe}";
                var message = $@"
                    <html>
                    <body>
                        <h2>Contract Expiration Notice</h2>
                        <p>Hello <strong>{employee.FirstName} {employee.LastName}</strong>,</p>
                        <p>Your <strong>{contract.ContractType}</strong> contract expires in <strong>{timeframe}</strong> ({days} days).</p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p><strong>Contract Details:</strong></p>
                            <p><strong>End Date:</strong> {contract.ContractEndDate:MMMM d, yyyy}</p>
                            <p><strong>Contract Type:</strong> {contract.ContractType}</p>
                            <p><strong>Category:</strong> {contract.ContractCategory ?? "Not specified"}</p>
                            <p><strong>Days Remaining:</strong> {days} days</p>
                        </div>
                        <p>Please contact the HR department to discuss contract renewal or extension options.</p>
                        <p>If you have any questions, please reach out to your supervisor or the HR team.</p>
                        <br>
                        <p><em>This is an automated notification from BCAS HRIS System.</em></p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(employee.Email, subject, message);
                Console.WriteLine($"✅ EMAIL SENT: {timeframe} expiration notice to {employee.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED to send email to {employee.Email}: {ex.Message}");
            }
        }

        private async Task SendExpiredNotification(tblEmployees employee, tblContracts contract)
        {
            try
            {
                var subject = "URGENT: Contract Has Expired";
                var message = $@"
                    <html>
                    <body>
                        <h2 style='color: #dc3545;'>🚨 CONTRACT EXPIRED</h2>
                        <p>Hello <strong>{employee.FirstName} {employee.LastName}</strong>,</p>
                        <p>Your <strong>{contract.ContractType}</strong> contract has <strong style='color: #dc3545;'>EXPIRED</strong>.</p>
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #dc3545;'>
                            <p><strong>Contract Details:</strong></p>
                            <p><strong>End Date:</strong> {contract.ContractEndDate:MMMM d, yyyy}</p>
                            <p><strong>Contract Type:</strong> {contract.ContractType}</p>
                            <p><strong>Category:</strong> {contract.ContractCategory ?? "Not specified"}</p>
                            <p><strong>Status:</strong> <span style='color: #dc3545; font-weight: bold;'>EXPIRED</span></p>
                        </div>
                        <p><strong>IMMEDIATE ACTION REQUIRED:</strong></p>
                        <p>❌ Your contract has expired and requires immediate attention.</p>
                        <p>📞 Please contact HR immediately to resolve this matter.</p>
                        <p>⚠️ Failure to address this may affect your employment status.</p>
                        <br>
                        <p><em>This is an automated urgent notification from BCAS HRIS System.</em></p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(employee.Email, subject, message);
                Console.WriteLine($"✅ EMAIL SENT: Contract expired notice to {employee.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED to send expired notice to {employee.Email}: {ex.Message}");
            }
        }
    }
}