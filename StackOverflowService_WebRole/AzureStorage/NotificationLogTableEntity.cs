using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService_WebRole.AzureStorage
{
    public class NotificationLogTableEntity : TableEntity
    {
        public string AnswerId { get; set; }
        public int SentEmailsCount { get; set; }

        public NotificationLogTableEntity() { }

        public NotificationLogTableEntity(string answerId)
        {
            this.PartitionKey = answerId;
            this.RowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);

            this.AnswerId = answerId;
        }
    }
}