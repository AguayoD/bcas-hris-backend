
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.DTOs.UsersDTO;
using Models.Models;
using Models.Utilities;
using Repositories.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Repositories.Services
{
    public class AuthenticationService
    {
       // private readonly IConfigurationRoot _config = new ConfigurationUtility().config;
        public readonly IConfigurationRoot _config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json")
        .Build();

        tblUsersRepository _tblUserRepository = new tblUsersRepository();

        public string GenerateToken(UserRolesDTO user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Username),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("EmployeeId", user.EmployeeId.ToString()),
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    if (role.RoleName != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                    }
                }
            }

            var jwtToken = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(_config["Jwt:Key"])
                ),
                SecurityAlgorithms.HmacSha256Signature)
        );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);

        }
        public async Task<UserRolesDTO> UserLogin(UserLoginDTO userLogin)
        {
            tblUsers user = await _tblUserRepository.GetByUsername(userLogin.Username);

            if (user != null)
            {
                UserRolesDTO userRoles = await _tblUserRepository.GetByIdWithRoles(user.UserId ?? 0);
                byte[] salt = Convert.FromBase64String(user.Salt);
                var Encrypted = ToHash(userLogin.Password, salt);
                if (Encrypted == user.PasswordHash)
                {
                    return userRoles;
                }
            }
            throw new Exception("Invalid Username or Password");
        }
        public byte[] GenerateRandomSalt()
        {
            return RandomNumberGenerator.GetBytes(128 / 8);
        }
        public string ToHash(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        }


    }
}
