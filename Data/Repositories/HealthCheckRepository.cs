using Data.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class HealthCheckRepository
    {
        private CloudTable _table;

        public HealthCheckRepository(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("HealthCheck");
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task InsertHealthCheckAsync(HealthCheck entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            await _table.ExecuteAsync(insertOperation);
        }

        public async Task<List<HealthCheck>> RetrieveAllHealthChecksAsync(DateTime fromTime)
        {
            var allEntities = new List<HealthCheck>();
            TableContinuationToken token = null;

            string filter = TableQuery.GenerateFilterConditionForDate(
                "Timestamp", QueryComparisons.GreaterThanOrEqual, fromTime);

            var query = new TableQuery<HealthCheck>().Where(filter);

            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                allEntities.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);

            return allEntities;
        }

        public async Task<List<HealthCheck>> GetRecentHealthChecksAsync(string serviceName, TimeSpan timeSpan)
        {
            var allEntities = new List<HealthCheck>();
            TableContinuationToken token = null;
            DateTime fromTime = DateTime.UtcNow.Subtract(timeSpan);

            // Filter po vremenu (Timestamp je automatski DateTimeOffset u TableEntity)
            string timeFilter = TableQuery.GenerateFilterConditionForDate(
                "Timestamp", QueryComparisons.GreaterThanOrEqual, fromTime);

            string serviceFilter = TableQuery.GenerateFilterCondition(
                "ServiceName", QueryComparisons.Equal, serviceName);

            string combinedFilter = TableQuery.CombineFilters(timeFilter, TableOperators.And, serviceFilter);

            var query = new TableQuery<HealthCheck>().Where(combinedFilter);

            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                allEntities.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);

            return allEntities;
        }
    }
}