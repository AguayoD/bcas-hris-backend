using Models.DTOs.UsersDTO;
using Models.Models;
using Repositories.Repositories;

namespace Repositories.Services
{
    public class tblUsersService
    {
        private readonly tblUsersRepository _tblUsersRepository = new tblUsersRepository();
        private readonly AuthenticationService _authenticationService = new AuthenticationService();
        private readonly EmailService _emailService = new EmailService();

        public async Task<IEnumerable<tblUsers>> GetAll()
        {
            return await _tblUsersRepository.GetAll();
        }

        public async Task<tblUsers> GetById(int id)
        {
            return await _tblUsersRepository.GetById(id);
        }

        public async Task<tblUsers> GetByUsername(string username)
        {
            return await _tblUsersRepository.GetByUsername(username);
        }

        public async Task<tblUsers> Insert(UserInsertDTO userData)
        {
            try
            {
                var salt = _authenticationService.GenerateRandomSalt();
                var newPassword = _authenticationService.ToHash(userData.NewPassword, salt);

                var newUser = new tblUsers()
                {
                    EmployeeId = userData.EmployeeId,
                    RoleId = userData.RoleId,
                    UserName = userData.Username,
                    PasswordHash = newPassword,
                    Salt = Convert.ToBase64String(salt),
                };

                var insertedUser = await _tblUsersRepository.Insert(newUser);

                if (insertedUser?.UserId == null)
                {
                    throw new Exception("Failed to insert user");
                }

                // Send email with credentials if email is provided
                if (!string.IsNullOrEmpty(userData.Email))
                {
                    await SendUserCredentialsEmail(userData.Email, userData.Username, userData.NewPassword, userData.FirstName, userData.LastName);
                }

                return insertedUser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting user with roles: {ex.Message}", ex);
            }
        }

        private async Task SendUserCredentialsEmail(string email, string username, string password, string? firstName, string? lastName)
        {
            try
            {
                var employeeName = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                    ? $"{firstName} {lastName}"
                    : "User";

                var subject = "Your BCAS HRIS Account Credentials";
                var messageBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; margin: 20px; }}
                            .credentials-box {{ background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #00883e; }}
                            .login-button {{ background-color: #00883e; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block; }}
                            .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <h2>Welcome to BCAS HRIS, {employeeName}!</h2>
                        <p>Your account has been successfully created.</p>
                        
                        <div class='credentials-box'>
                            <p><strong>Username:</strong> {username}</p>
                            <p><strong>Password:</strong> {password}</p>
                        </div>
                        
                        <p>Please log in at your earliest convenience and change your password for security.</p>
                        <p><a href='http://localhost:5173' class='login-button'>Login to System</a></p>
                        
                        <div class='footer'>
                            <p><strong>Security Notice:</strong> For your security, please change your password after first login.</p>
                            <p>If you did not request this account, please contact the administrator immediately.</p>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(email, subject, messageBody);
                Console.WriteLine($"Credentials email sent to {email}");
            }
            catch (Exception ex)
            {
                // Log but don't fail user creation
                Console.WriteLine($"Failed to send credentials email to {email}: {ex.Message}");
            }
        }

        public async Task<UserRolesDTO> GetByIdWithRoles(int id)
        {
            return await _tblUsersRepository.GetByIdWithRoles(id);
        }

        public async Task<tblUsers> Update(tblUsers tblemployee)
        {
            return await _tblUsersRepository.Update(tblemployee);
        }

        public async Task<tblUsers> DeleteById(int id)
        {
            return await _tblUsersRepository.DeleteById(id);
        }

        // FIXED: GeneratePasswordHash method
        public (string hashedPassword, string salt) GeneratePasswordHash(string newPassword)
        {
            var saltBytes = _authenticationService.GenerateRandomSalt();
            var hashedPassword = _authenticationService.ToHash(newPassword, saltBytes);
            return (hashedPassword, Convert.ToBase64String(saltBytes));
        }

        // ADDED - Password reset functionality
        public async Task<string> ForgotPasswordAsync(string email)
        {
            // Get user by email
            var user = await _tblUsersRepository.GetByEmail(email);
            if (user == null)
            {
                // Return success to prevent user enumeration attacks.
                return "If an account exists, a password reset link has been sent.";
            }

            // Generate a secure, single-use token
            string token = Guid.NewGuid().ToString("N");

            // Save reset token + expiry
            var saveResult = await _tblUsersRepository.SaveResetToken(user.UserId.Value, token);
            if (!saveResult)
            {
                return "failed token";
            }

            // Generate a secure reset link
            string resetLink = $"http://localhost:5173/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            // Send email with the reset link
            await _emailService.SendEmailAsync(email, "Password Reset",
                $"<p>Hello,</p><p>Please reset your password by clicking the link below:</p><p><a href='{resetLink}'>Reset Password</a></p><p>This link will expire in 30 minutes.</p>");

            return "Password reset link sent successfully.";
        }

        // UPDATED: ResetPasswordAsync method to use fixed GeneratePasswordHash
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _tblUsersRepository.GetUserByResetToken(token);
            if (user == null) return false;

            // Use the fixed GeneratePasswordHash method
            var (hashedPassword, salt) = GeneratePasswordHash(newPassword);

            var updated = await _tblUsersRepository.UpdatePassword(user.UserId.Value, hashedPassword, salt);

            if (!updated) return false;

            await _tblUsersRepository.ClearResetToken(user.UserId!.Value);
            return true;
        }
    }
}