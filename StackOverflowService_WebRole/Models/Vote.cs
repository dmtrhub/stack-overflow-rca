using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class Vote
    {
        public string Id { get; set; }
        public string AnswerId { get; set; }
        public string VotedByEmail { get; set; }
        public DateTime VotedAt { get; set; }
    }
}