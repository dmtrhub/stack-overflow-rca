using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.AzureStorage
{
    public class AnswerTableEntity : TableEntity
    {
        public AnswerTableEntity() { }

        public AnswerTableEntity(Answer answer)
        {
            PartitionKey = $"Answer_{answer.QuestionId}";
            RowKey = answer.Id ?? Guid.NewGuid().ToString();
            QuestionId = answer.QuestionId;
            Description = answer.Description;
            NumberOfVotes = answer.NumberOfVotes;
            AnsweredByEmail = answer.AnsweredByEmail;
            CreatedAt = answer.CreatedAt;
        }

        public string QuestionId { get; set; }
        public string Description { get; set; }
        public int NumberOfVotes { get; set; }
        public string AnsweredByEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}