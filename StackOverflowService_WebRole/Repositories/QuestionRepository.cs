using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StackOverflowService_WebRole.AzureStorage;
using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowService_WebRole.Repositories
{
    public class QuestionRepository
    {
        private readonly CloudTable _table;
        private readonly CloudTableClient _tableClient;

        public QuestionRepository()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            _tableClient = storageAccount.CreateCloudTableClient();
            _table = _tableClient.GetTableReference("Question");
            _table.CreateIfNotExists();
        }

        public async Task SaveQuestionAsync(Question question)
        {
            var insert = TableOperation.Insert(new QuestionTableEntity(question));
            await _table.ExecuteAsync(insert);
        }
        public async Task<List<QuestionTableEntity>> GetAllQuestionsAsync()
        {
            var query = new TableQuery<QuestionTableEntity>();
            var segment = await _table.ExecuteQuerySegmentedAsync(query, null);

            return segment.Results
                          .OrderByDescending(q => q.UpdatedAt)
                          .ToList();
        }

        public async Task<List<QuestionTableEntity>> GetQuestionsByDateRangeAsync(DateTime from, DateTime to)
        {
            string fromFilter = TableQuery.GenerateFilterConditionForDate("CreatedAt", QueryComparisons.GreaterThanOrEqual, from);
            string toFilter = TableQuery.GenerateFilterConditionForDate("CreatedAt", QueryComparisons.LessThanOrEqual, to);
            string combinedFilter = TableQuery.CombineFilters(fromFilter, TableOperators.And, toFilter);

            var query = new TableQuery<QuestionTableEntity>().Where(combinedFilter);
            var segment = await _table.ExecuteQuerySegmentedAsync(query, null);
            return segment.Results;
        }

        public async Task CloseQuestionAsync(string questionId, string topAnswerId)
        {
            var question = await GetQuestionByIdAsync(questionId);
            if (question != null)
            {
                question.IsClosed = true;
                question.TopAnswerId = topAnswerId;
                question.UpdatedAt = DateTime.UtcNow;

                var operation = TableOperation.Replace(new QuestionTableEntity
                {
                    PartitionKey = question.PartitionKey,
                    RowKey = question.RowKey,
                    Title = question.Title,
                    Description = question.Description,
                    PictureUrl = question.PictureUrl,
                    CreatedBy = question.CreatedBy,
                    TopAnswerId = question.TopAnswerId,
                    IsClosed = question.IsClosed,
                    CreatedAt = question.CreatedAt,
                    UpdatedAt = question.UpdatedAt,
                    ETag = "*"
                });

                await _table.ExecuteAsync(operation);
            }
        }

        public async Task<QuestionTableEntity> GetQuestionByIdAsync(string id)
        {
            var query = new TableQuery<QuestionTableEntity>().Where(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));

            var segment = await _table.ExecuteQuerySegmentedAsync(query, null);
            return segment.Results.FirstOrDefault();
        }

        public async Task UpdateQuestionAsync(QuestionTableEntity updatedEntity)
        {
            var operation = TableOperation.Replace(updatedEntity);
            await _table.ExecuteAsync(operation);
        }

        public async Task DeleteQuestionAsync(QuestionTableEntity entity)
        {
            var operation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(operation);
        }

        public async Task DeleteAnswersAndVotesForQuestionAsync(string questionId)
        {
            var answerTable = _tableClient.GetTableReference("Answer");
            var voteTable = _tableClient.GetTableReference("Vote");

            await answerTable.CreateIfNotExistsAsync();
            await voteTable.CreateIfNotExistsAsync();

            var answerPartitionKey = $"Answer_{questionId}";
            var answerQuery = new TableQuery<AnswerTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, answerPartitionKey));

            TableContinuationToken token = null;
            do
            {
                var segment = await answerTable.ExecuteQuerySegmentedAsync(answerQuery, token);
                token = segment.ContinuationToken;

                foreach (var answer in segment.Results)
                {
                    var votePartitionKey = $"Vote_{answer.RowKey}";
                    var voteQuery = new TableQuery<VoteTableEntity>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, votePartitionKey));

                    TableContinuationToken voteToken = null;
                    do
                    {
                        var voteSegment = await voteTable.ExecuteQuerySegmentedAsync(voteQuery, voteToken);
                        voteToken = voteSegment.ContinuationToken;

                        foreach (var vote in voteSegment.Results)
                        {
                            var deleteVoteOp = TableOperation.Delete(vote);
                            await voteTable.ExecuteAsync(deleteVoteOp);
                        }
                    } while (voteToken != null);

                    var deleteAnswerOp = TableOperation.Delete(answer);
                    await answerTable.ExecuteAsync(deleteAnswerOp);
                }
            } while (token != null);
        }
        public async Task<List<QuestionTableEntity>> GetQuestionsByEmailAsync(string email)
        {
            string filter = TableQuery.GenerateFilterCondition("CreatedBy", QueryComparisons.Equal, email);
            var query = new TableQuery<QuestionTableEntity>().Where(filter);

            TableContinuationToken token = null;
            var results = new List<QuestionTableEntity>();

            do
            {
                var segment = await _table.ExecuteQuerySegmentedAsync(query, token);
                token = segment.ContinuationToken;
                results.AddRange(segment.Results);
            } while (token != null);

            return results;
        }
    }
}