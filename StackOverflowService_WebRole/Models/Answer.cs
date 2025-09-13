using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class Answer
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string Description { get; set; }
        public int NumberOfVotes { get; set; }
        public string AnsweredByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}