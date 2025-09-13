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
    public class VoteRepository
    {
        private readonly CloudTable _table;

        public VoteRepository()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
            var client = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
            _table = client.GetTableReference("Vote");
            _table.CreateIfNotExists();
        }

        public async Task<bool> HasUserAlreadyVotedAsync(string answerId, string userEmail)
        {
            var retrieve = TableOperation.Retrieve<VoteTableEntity>($"Vote_{answerId}", userEmail);
            var result = await _table.ExecuteAsync(retrieve);
            return result.Result != null;
        }

        public async Task<bool> SaveVoteAsync(string answerId, string userEmail)
        {
            if (await HasUserAlreadyVotedAsync(answerId, userEmail))
                return false;

            var vote = new VoteTableEntity(answerId, userEmail);
            var insert = TableOperation.Insert(vote);
            await _table.ExecuteAsync(insert);
            return true;
        }
        public async Task<bool> DeleteVoteAsync(string answerId, string userEmail)
        {
            var retrieve = TableOperation.Retrieve<VoteTableEntity>($"Vote_{answerId}", userEmail);
            var result = await _table.ExecuteAsync(retrieve);

            if (result.Result is VoteTableEntity voteEntity)
            {
                var delete = TableOperation.Delete(voteEntity);
                await _table.ExecuteAsync(delete);
                return true;
            }

            return false;
        }
    }
}