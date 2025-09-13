using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.DTOs
{
    public class ValidationResponseDto
    {
        public bool Success { get; set; } = false;
        public List<FieldError> Errors { get; set; } = new List<FieldError>();
    }

    public class FieldError
    {
        public string Field { get; set; }
        public string Message { get; set; }
    }
}