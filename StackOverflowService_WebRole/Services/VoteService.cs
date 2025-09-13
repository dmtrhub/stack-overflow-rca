using StackOverflowService_WebRole.DTOs;
using StackOverflowService_WebRole.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace StackOverflowService_WebRole.Services
{
    public class VoteService
    {
        private readonly VoteRepository _voteRepository;
        private readonly AnswerRepository _answerRepository;
        public VoteService(VoteRepository voteRepository, AnswerRepository answerRepository)
        {
            _voteRepository = voteRepository;
            _answerRepository = answerRepository;
        }

        public async Task<bool> CreateVoteAsync(CreateVoteDto dto)
        {
            var ret = await _answerRepository.GetAnswerByIdAsync(dto.AnswerId);
            if (ret == null)
                return false;

            var ret2 = await _voteRepository.SaveVoteAsync(dto.AnswerId, dto.VotedByEmail);
            if (ret2 == false)
                return false;

            ret.NumberOfVotes++;
            await _answerRepository.UpdateAnswerAsync(ret);

            return true;
        }

        public async Task<bool> DeleteVoteAsync(string answerId, string userEmail)
        {
            var answer = await _answerRepository.GetAnswerByIdAsync(answerId);
            if (answer == null)
                return false;

            var deleted = await _voteRepository.DeleteVoteAsync(answerId, userEmail);
            if (!deleted)
                return false;

            answer.NumberOfVotes = Math.Max(0, answer.NumberOfVotes - 1);
            await _answerRepository.UpdateAnswerAsync(answer);

            return true;
        }

        public async Task<bool> HasUserVotedAsync(string answerId, string userEmail)
        {
            return await _voteRepository.HasUserAlreadyVotedAsync(answerId, userEmail);
        }
    }
}