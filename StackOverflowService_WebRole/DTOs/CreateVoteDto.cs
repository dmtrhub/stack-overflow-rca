using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.DTOs
{
    public class CreateVoteDto
    {
        public string AnswerId { get; set; }
        public string VotedByEmail { get; set; }
    }
}