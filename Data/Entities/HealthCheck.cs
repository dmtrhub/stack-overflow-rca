using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Data.Entities
{
    public class HealthCheck : TableEntity
    {
        public HealthCheck()
        { }

        public HealthCheck(string serviceName, DateTime timestamp)
        {
            PartitionKey = serviceName;
            RowKey = timestamp.Ticks.ToString();
        }

        public string Status { get; set; }
        public string ServiceName { get; set; }
    }
}