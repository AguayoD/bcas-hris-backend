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
    public class UsersController : ControllerBase
    {
        private readonly tblUsersService _tbluserService = new tblUsersService();

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
                    // Keep existing password hash and salt if no new password provided
                    PasswordHash = existingUser.PasswordHash,
                    Salt = existingUser.Salt
                };

                // If new password is provided, generate new hash and salt
                if (!string.IsNullOrEmpty(userUpdateDTO.NewPassword))
                {
                    // Assuming your service has a method to hash passwords
                    var (hashedPassword, salt) = _tbluserService.GeneratePasswordHash(userUpdateDTO.NewPassword);
                    userToUpdate.PasswordHash = hashedPassword;
                    userToUpdate.Salt = salt;
                }

                var updatedUser = await _tbluserService.Update(userToUpdate);
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
                return Ok(new { Message = "User deleted successfully.", DeletedUser = deletedUser });
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

                return Ok(new { Message = "User activated successfully.", User = activatedUser });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //ADDED
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            var success = await _tbluserService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "If an account exists, a reset link has been sent." });
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var success = await _tbluserService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest(new { message = "Invalid or expired token." });

            return Ok(new { message = "Password reset successful." });
        }
    }
}