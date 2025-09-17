using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowApp_Data.Entities;

namespace StackOverflowApp_Data.Repositories
{
    public class AlertEmailRepository
    {
        private readonly CloudTable _table;

        public AlertEmailRepository(string connectionString)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var client = account.CreateCloudTableClient();
            _table = client.GetTableReference("AlertEmails");
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
                emails.AddRange(segment.Results.Select(r => r.Email));
                token = segment.ContinuationToken;
            } while (token != null);

            return emails;
        }
    }
}