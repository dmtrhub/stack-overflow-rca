using StackOverflowService_WebRole.Models;
using StackOverflowService_WebRole.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using StackOverflowService_WebRole.DTOs;

namespace StackOverflowService_WebRole.Services
{
    public class AnswerService
    {
        private readonly AnswerRepository _answerRepository;

        public AnswerService(AnswerRepository answerRepository)
        {
            _answerRepository = answerRepository;
        }

        public async Task<ValidationResponseDto> CreateAsync(CreateAnswerDto dto, string email)
        {
            var errors = new List<FieldError>();

            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add(new FieldError { Field = "Description", Message = "Description is required." });
            if (string.IsNullOrWhiteSpace(dto.QuestionId))
                errors.Add(new FieldError { Field = "QuestionId", Message = "QuestionId is required." });

            if (errors.Any())
            {
                return new ValidationResponseDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            var answer = new Answer()
            {
                Id = Guid.NewGuid().ToString(),
                QuestionId = dto.QuestionId,
                Description = dto.Description,
                NumberOfVotes = 0,
                AnsweredByEmail = email,
                CreatedAt = DateTime.UtcNow,
            };

            await _answerRepository.SaveAnswerAsync(answer);

            return new ValidationResponseDto
            {
                Success = true,
                Errors = new List<FieldError>()
            };
        }
        public async Task<List<AnswerDto>> GetAnswersByQuestionIdAsync(string questionId)
        {
            var entities = await _answerRepository.GetAnswersByQuestionIdAsync(questionId);

            return entities
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new AnswerDto
                {
                    Id = e.RowKey,
                    QuestionId = e.QuestionId,
                    Description = e.Description,
                    NumberOfVotes = e.NumberOfVotes,
                    AnsweredByEmail = e.AnsweredByEmail,
                    CreatedAt = e.CreatedAt
                })
                .ToList();
        }
    }
}