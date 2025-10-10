
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
        public async new Task<tblUsers> Insert(UserInsertDTO userData)
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

                return insertedUser;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting user with roles: {ex.Message}", ex);
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

        public (string? hashedPassword, string? salt) GeneratePasswordHash(string newPassword)
        {
            throw new NotImplementedException();
        }

        //ADDED
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

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _tblUsersRepository.GetUserByResetToken(token);
            if (user == null) return false;

            var salt = _authenticationService.GenerateRandomSalt();
            var passwordHash = _authenticationService.ToHash(newPassword, salt);

            var updated = await _tblUsersRepository.UpdatePassword(user.UserId.Value, passwordHash, Convert.ToBase64String(salt));

            if (!updated) return false;

            await _tblUsersRepository.ClearResetToken(user.UserId!.Value);
            return true;
        }
    }
}
