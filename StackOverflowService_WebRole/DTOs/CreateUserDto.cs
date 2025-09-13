using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.DTOs
{
    public class CreateUserDto
    {
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public HttpPostedFile ProfileImage { get; set; }
    }
}