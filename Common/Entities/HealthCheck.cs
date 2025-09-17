using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace StackOverflowApp_Data.Entities
{
    public class HealthCheck : TableEntity
    {
        public HealthCheck()
        { }

        public HealthCheck(string serviceName, DateTime timestamp)
        {
            PartitionKey = serviceName;
            RowKey = timestamp.Ticks.ToString();
            TimestampUtc = timestamp;
        }

        public DateTime TimestampUtc { get; set; }
        public string Status { get; set; }
        public string ServiceName { get; set; }
    }
}