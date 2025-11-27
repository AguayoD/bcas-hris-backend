// Controllers/UsersController.cs
using BCAS_HRMSbackend.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs.UsersDTO;
using Models.Models;
using Repositories.Service;
using Repositories.Services;

namespace BcasHRMS_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly tblUsersService _tbluserService = new tblUsersService();

        public UsersController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserInsertDTO userDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingUser = await _tbluserService.GetByUsername(userDTO.Username);
                if (existingUser != null)
                {
                    return Conflict("Username already exists. Please choose a different username.");
                }

                var newUser = await _tbluserService.Insert(userDTO);

                // Log the INSERT action
                if (newUser?.UserId != null)
                {
                    await LogActionAsync("tblUsers", "INSERT", newUser.UserId.ToString(), null, newUser);
                }

                return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO userUpdateDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (id != userUpdateDTO.UserId)
                    return BadRequest("User ID in URL does not match User ID in request body.");

                var existingUser = await _tbluserService.GetById(id);
                if (existingUser == null)
                    return NotFound("User not found.");

                // Check if username is being changed and if new username already exists
                if (existingUser.UserName != userUpdateDTO.UserName)
                {
                    var userWithSameUsername = await _tbluserService.GetByUsername(userUpdateDTO.UserName);
                    if (userWithSameUsername != null && userWithSameUsername.UserId != id)
                    {
                        return Conflict("Username already exists. Please choose a different username.");
                    }
                }

                // Convert DTO to model
                var userToUpdate = new tblUsers
                {
                    UserId = userUpdateDTO.UserId,
                    EmployeeId = userUpdateDTO.EmployeeId,
                    RoleId = userUpdateDTO.RoleId,
                    UserName = userUpdateDTO.UserName,
                    IsActive = userUpdateDTO.IsActive,
                    PasswordHash = existingUser.PasswordHash,
                    Salt = existingUser.Salt
                };

                // If new password is provided, generate new hash and salt
                if (!string.IsNullOrEmpty(userUpdateDTO.NewPassword))
                {
                    var (hashedPassword, salt) = _tbluserService.GeneratePasswordHash(userUpdateDTO.NewPassword);
                    userToUpdate.PasswordHash = hashedPassword;
                    userToUpdate.Salt = salt;
                }

                var updatedUser = await _tbluserService.Update(userToUpdate);

                // Log the UPDATE action
                await LogActionAsync("tblUsers", "UPDATE", id.ToString(), existingUser, updatedUser);

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var existingUser = await _tbluserService.GetById(id);
                if (existingUser == null)
                    return NotFound("User not found.");

                var deletedUser = await _tbluserService.DeleteById(id);

                // Log the DELETE action
                await LogActionAsync("tblUsers", "DELETE", id.ToString(), existingUser, null);

                return Ok(new { Message = "User deleted successfully.", DeletedUser = deletedUser });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var data = await _tbluserService.GetAll();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var data = await _tbluserService.GetByIdWithRoles(id);
                if (data == null) return NotFound("User not found.");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                var existingUser = await _tbluserService.GetById(id);
                if (existingUser == null)
                    return NotFound("User not found.");

                // Set IsActive to false
                existingUser.IsActive = false;
                var deactivatedUser = await _tbluserService.Update(existingUser);

                // Log the UPDATE action for deactivation
                await LogActionAsync("tblUsers", "UPDATE", id.ToString(),
                    new { existingUser.IsActive }, new { deactivatedUser.IsActive });

                return Ok(new { Message = "User deactivated successfully.", User = deactivatedUser });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            try
            {
                var existingUser = await _tbluserService.GetById(id);
                if (existingUser == null)
                    return NotFound("User not found.");

                // Set IsActive to true
                existingUser.IsActive = true;
                var activatedUser = await _tbluserService.Update(existingUser);

                // Log the UPDATE action for activation
                await LogActionAsync("tblUsers", "UPDATE", id.ToString(),
                    new { existingUser.IsActive }, new { activatedUser.IsActive });

                return Ok(new { Message = "User activated successfully.", User = activatedUser });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest("Email is required.");

                var result = await _tbluserService.ForgotPasswordAsync(request.Email);

                // Always return success to prevent email enumeration
                return Ok(new { message = "If an account exists, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                // Still return success for security reasons
                Console.WriteLine($"Error in forgot password: {ex.Message}");
                return Ok(new { message = "If an account exists, a password reset link has been sent." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                    return BadRequest("Token and new password are required.");

                if (request.NewPassword.Length < 6)
                    return BadRequest("Password must be at least 6 characters long.");

                var result = await _tbluserService.ResetPasswordAsync(request.Token, request.NewPassword);

                if (!result)
                    return BadRequest("Invalid or expired reset token.");

                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error resetting password: {ex.Message}");
            }
        }
    }
}