using Data.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class AlertEmailRepository
    {
        private readonly CloudTable _table;

        public AlertEmailRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("AlertEmails");
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task<List<string>> GetAllEmailsAsync()
        {
            var emails = new List<string>();
            TableQuery<AlertEmail> query = new TableQuery<AlertEmail>();
            TableContinuationToken token = null;

            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;

                foreach (var entity in segment.Results)
                {
                    emails.Add(entity.Email);
                }
            } while (token != null);

            return emails;
        }

        public async Task AddEmailAsync(string email)
        {
            var entity = new AlertEmail(email);
            var insertOperation = TableOperation.InsertOrReplace(entity);
            await _table.ExecuteAsync(insertOperation);
        }

        public async Task RemoveEmailAsync(string email)
        {
            var retrieveOperation = TableOperation.Retrieve<AlertEmail>("AlertEmails", email);
            var retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            if (retrievedResult.Result is AlertEmail entity)
            {
                var deleteOperation = TableOperation.Delete(entity);
                await _table.ExecuteAsync(deleteOperation);
            }
        }
    }
}