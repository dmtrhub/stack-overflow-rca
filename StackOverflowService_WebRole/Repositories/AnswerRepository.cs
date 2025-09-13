using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using StackOverflowService_WebRole.AzureStorage;
using Microsoft.Azure;

namespace StackOverflowService_WebRole.Repositories
{
    public class AnswerRepository
    {
        private readonly CloudTable _table;

        public AnswerRepository()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var client = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
            _table = client.GetTableReference("Answer");
            _table.CreateIfNotExists();
        }

        public async Task SaveAnswerAsync(Answer answer)
        {
            var entity = new AnswerTableEntity(answer);
            var insert = TableOperation.Insert(entity);
            await _table.ExecuteAsync(insert);
        }

        public async Task<List<AnswerTableEntity>> GetAnswersByQuestionIdAsync(string questionId)
        {
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"Answer_{questionId}");
            var query = new TableQuery<AnswerTableEntity>().Where(filter);
            var segment = await _table.ExecuteQuerySegmentedAsync(query, null);
            return segment.Results;
        }
        public async Task<AnswerTableEntity> GetAnswerByIdAsync(string answerId)
        {
            var filter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, answerId);
            var query = new TableQuery<AnswerTableEntity>().Where(filter);
            var segment = await _table.ExecuteQuerySegmentedAsync(query, null);
            return segment.Results.FirstOrDefault();
        }
        public async Task UpdateAnswerAsync(AnswerTableEntity updatedEntity)
        {
            updatedEntity.ETag = "*";
            var operation = TableOperation.Replace(updatedEntity);
            await _table.ExecuteAsync(operation);
        }
    }
}