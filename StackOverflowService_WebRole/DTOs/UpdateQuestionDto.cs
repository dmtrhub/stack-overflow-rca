using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.DTOs
{
    public class UpdateQuestionDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public HttpPostedFile QuestionImage { get; set; }
    }
}