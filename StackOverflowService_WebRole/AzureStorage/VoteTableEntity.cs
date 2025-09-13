using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.AzureStorage
{
    public class VoteTableEntity : TableEntity
    {
        public VoteTableEntity() { }

        public VoteTableEntity(string answerId, string userEmail)
        {
            PartitionKey = $"Vote_{answerId}";
            RowKey = userEmail;
            AnswerId = answerId;
            VotedByEmail = userEmail;
            VotedAt = DateTime.UtcNow;
        }

        public string AnswerId { get; set; }
        public string VotedByEmail { get; set; }
        public DateTime VotedAt { get; set; }
    }
}