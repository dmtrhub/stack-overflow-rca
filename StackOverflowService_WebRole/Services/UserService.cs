using StackOverflowService_WebRole.AzureStorage;
using StackOverflowService_WebRole.DTOs;
using StackOverflowService_WebRole.Models;
using StackOverflowService_WebRole.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace StackOverflowService_WebRole.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepo;
        private readonly ProfilePictureBlobService _profilePictureBlobService;

        public UserService(UserRepository userRepo, ProfilePictureBlobService profilePictureBlobService)
        {
            _userRepo = userRepo;
            _profilePictureBlobService = profilePictureBlobService;
        }

        public async Task<ValidationResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            var errors = new List<FieldError>();

            // Validacije polja
            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add(new FieldError { Field = "fullName", Message = "Full name is required." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add(new FieldError { Field = "email", Message = "Email is required." });
            else if (!dto.Email.Contains("@"))
                errors.Add(new FieldError { Field = "email", Message = "Invalid email format." });

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                errors.Add(new FieldError { Field = "password", Message = "Password must be at least 6 characters." });

            if (dto.ProfileImage == null)
                errors.Add(new FieldError { Field = "profileImage", Message = "Profile image is required." });

            // Ako ima validacionih grešaka
            if (errors.Any())
            {
                return new ValidationResponseDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Provera duplikata email-a
            if (await _userRepo.EmailExistsAsync(dto.Email))
            {
                errors.Add(new FieldError { Field = "email", Message = "Email already exists." });

                return new ValidationResponseDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            // Upload slike
            var profileUrl = await _profilePictureBlobService.UploadFileAsync(dto.ProfileImage);

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                FullName = dto.FullName,
                Gender = dto.Gender,
                Country = dto.Country,
                City = dto.City,
                Address = dto.Address,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                ProfilePictureUrl = profileUrl
            };

            await _userRepo.SaveUserAsync(user);

            return new ValidationResponseDto
            {
                Success = true,
                Errors = new List<FieldError>()
            };
        }
        public async Task<UserTableEntity> GetUserByEmailAsync(string email)
        {
            return await _userRepo.GetByEmailAsync(email);
        }
        public async Task<string> GenerateJwtToken(UserTableEntity user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("070342f4-b749-4e1c-ab79-e73cfc2348a2");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.RowKey),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = "http://localhost:51400/",
                Audience = "http://localhost:5173/",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
        public async Task<ValidationResponseDto> UpdateUserAsync(string email, UpdateUserDto dto)
        {
            var errors = new List<FieldError>();

            var userEntity = await _userRepo.GetByEmailAsync(email);
            if (userEntity == null)
            {
                errors.Add(new FieldError { Field = "email", Message = "User not found." });
                return new ValidationResponseDto { Success = false, Errors = errors };
            }
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 6)
                {
                    errors.Add(new FieldError { Field = "password", Message = "Password must be at least 6 characters." });
                    return new ValidationResponseDto { Success = false, Errors = errors };
                }

                userEntity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            // update polja
            userEntity.FullName = dto.FullName ?? userEntity.FullName;
            userEntity.Gender = dto.Gender ?? userEntity.Gender;
            userEntity.Country = dto.Country ?? userEntity.Country;
            userEntity.City = dto.City ?? userEntity.City;
            userEntity.Address = dto.Address ?? userEntity.Address;

            // ako ima nova profilna slika
            if (dto.ProfileImage != null)
            {
                // Ako korisnik već ima sliku, izbriši staru iz blob-a
                if (!string.IsNullOrEmpty(userEntity.ProfilePictureUrl))
                {
                    await _profilePictureBlobService.DeleteFileAsync(userEntity.ProfilePictureUrl);
                }

                // Upload nove slike
                var url = await _profilePictureBlobService.UploadFileAsync(dto.ProfileImage);
                userEntity.ProfilePictureUrl = url;
            }

            await _userRepo.UpdateUserAsync(userEntity);

            return new ValidationResponseDto { Success = true };
        }
    }
}