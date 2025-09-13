using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.AzureStorage
{
    public class QuestionTableEntity : TableEntity
    {
        public QuestionTableEntity() { }

        public QuestionTableEntity(Question question)
        {
            PartitionKey = "Question";
            RowKey = question.Id ?? Guid.NewGuid().ToString();
            Title = question.Title;
            Description = question.Description;
            PictureUrl = question.PictureUrl;
            CreatedBy = question.CreatedBy;
            TopAnswerId = question.TopAnswerId;
            IsClosed = question.IsClosed;
            CreatedAt = question.CreatedAt;
            UpdatedAt = question.UpdatedAt;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public string PictureUrl { get; set; }
        public string CreatedBy { get; set; }
        public string TopAnswerId { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}