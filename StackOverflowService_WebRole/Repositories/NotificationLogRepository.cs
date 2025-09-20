using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowService_WebRole.AzureStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowService_WebRole.Repositories
{
    public class NotificationLogRepository
    {
        private readonly CloudTable _table;

        public NotificationLogRepository()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var client = storageAccount.CreateCloudTableClient();
            _table = client.GetTableReference("NotificationLogs"); 
            _table.CreateIfNotExists();
        }

        public async Task SaveLogAsync(NotificationLogTableEntity log)
        {
            var insertOperation = TableOperation.Insert(log);
            await _table.ExecuteAsync(insertOperation);
        }
    }
}