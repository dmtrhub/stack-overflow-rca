using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.Models
{
    public class Question
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public string CreatedBy { get; set; } // e-mail korisnika
        public string TopAnswerId { get; set; }
        public bool IsClosed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }
}