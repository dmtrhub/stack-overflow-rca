using StackOverflowService_WebRole.DTOs;
using StackOverflowService_WebRole.Models;
using StackOverflowService_WebRole.Repositories;
using StackOverflowService_WebRole.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace StackOverflowService_WebRole.Controllers
{
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public class UserController : ApiController
        {
            private readonly UserService _userService;

            public UserController()
            {
                var userRepo = new UserRepository();
                var blobService = new ProfilePictureBlobService();
                _userService = new UserService(userRepo, blobService);
            }

            [HttpPost]
            [Route("api/users/create")]
            public async Task<IHttpActionResult> Create()
            {
                var dto = new CreateUserDto
                {
                    FullName = HttpContext.Current.Request.Form["fullName"],
                    Gender = HttpContext.Current.Request.Form["gender"],
                    Country = HttpContext.Current.Request.Form["country"],
                    City = HttpContext.Current.Request.Form["city"],
                    Address = HttpContext.Current.Request.Form["address"],
                    Email = HttpContext.Current.Request.Form["email"],
                    Password = HttpContext.Current.Request.Form["password"],
                    ProfileImage = HttpContext.Current.Request.Files["profileImage"]
                };

                var result = await _userService.CreateUserAsync(dto);

                if (result.Success)
                    return Ok("User created successfully.");
                else
                    return Content(HttpStatusCode.BadRequest, result);
            }
            [HttpPost]
            [Route("api/users/login")]
            public async Task<IHttpActionResult> Login(LoginDto dto)
            {
                var user = await _userService.GetUserByEmailAsync(dto.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                {
                    return Content(HttpStatusCode.BadRequest, new { message = "Invalid credentials." });
                }

                var token = await _userService.GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    message = "ok"
                });
            }
            [Authorize]
            [HttpGet]
            [Route("api/users/test/autorize")]
            public IHttpActionResult GetProfile()
            {
                var identity = (ClaimsIdentity)User.Identity;
                var email = identity.FindFirst(ClaimTypes.Email)?.Value;
                var id = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Ok(new { email, id });
            }
            [Authorize]
            [HttpGet]
            [Route("api/users/profilepicture")]
            public async Task<IHttpActionResult> GetProfilePicture()
            {
                var identity = (ClaimsIdentity)User.Identity;
                var email = identity.FindFirst(ClaimTypes.Email)?.Value;

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound();

                return Ok(new
                {
                    success = true,
                    profilePictureUrl = user.ProfilePictureUrl
                });
            }
            [Authorize]
            [HttpGet]
            [Route("api/users/profile")]
            public async Task<IHttpActionResult> GetUserProfile()
            {
                var identity = (ClaimsIdentity)User.Identity;
                var email = identity.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized();

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return Content(HttpStatusCode.NotFound, "User not found.");

                var dto = new UserProfileDto
                {
                    FullName = user.FullName,
                    Gender = user.Gender,
                    Country = user.Country,
                    City = user.City,
                    Address = user.Address,
                    ProfilePictureUrl = user.ProfilePictureUrl
                };

                return Ok(dto);
            }

            [Authorize]
            [HttpPut]
            [Route("api/users/update")]
            public async Task<IHttpActionResult> Update()
            {
                var identity = (ClaimsIdentity)User.Identity;
                var email = identity.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return Unauthorized();

                var dto = new UpdateUserDto
                {
                    FullName = HttpContext.Current.Request.Form["fullName"],
                    Gender = HttpContext.Current.Request.Form["gender"],
                    Country = HttpContext.Current.Request.Form["country"],
                    City = HttpContext.Current.Request.Form["city"],
                    Address = HttpContext.Current.Request.Form["address"],
                    Password = HttpContext.Current.Request.Form["password"],
                    ProfileImage = HttpContext.Current.Request.Files["profileImage"]
                };

                var result = await _userService.UpdateUserAsync(email, dto);

                if (result.Success)
                    return Ok("User updated.");
                else
                    return Content(HttpStatusCode.BadRequest, result);
            }
        }
}