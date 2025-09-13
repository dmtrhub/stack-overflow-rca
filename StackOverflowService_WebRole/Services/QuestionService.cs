using StackOverflowService_WebRole.AzureStorage;
using StackOverflowService_WebRole.DTOs;
using StackOverflowService_WebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using StackOverflowService_WebRole.Repositories;

namespace StackOverflowService_WebRole.Services
{
    public class QuestionService
    {
        private readonly QuestionRepository _questionRepository;
        private readonly QuestionPictureBlobService _pictureBlobService;

        public QuestionService(QuestionRepository questionRepository, QuestionPictureBlobService pictureBlobService)
        {
            _questionRepository = questionRepository;
            _pictureBlobService = pictureBlobService;
        }

        public async Task<ValidationResponseDto> CreateQuestionAsync(CreateQuestionDto dto)
        {
            var errors = new List<FieldError>();

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add(new FieldError { Field = "title", Message = "Title is required." });
            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add(new FieldError { Field = "description", Message = "Description is required." });
            if (dto.QuestionImage == null)
                errors.Add(new FieldError { Field = "questionImage", Message = "Question image is required." });

            if (errors.Any())
            {
                return new ValidationResponseDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            var profileUrl = await _pictureBlobService.UploadFileAsync(dto.QuestionImage);

            var question = new Question()
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                PictureUrl = profileUrl,
                CreatedBy = dto.CreatedBy,
                TopAnswerId = null,
                IsClosed = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            await _questionRepository.SaveQuestionAsync(question);

            return new ValidationResponseDto
            {
                Success = true,
                Errors = new List<FieldError>()
            };
        }
        public async Task<List<QuestionDto>> GetAllAsync()
        {
            var questions = await _questionRepository.GetAllQuestionsAsync();
            return questions.Select(MapToDto).ToList();
        }
        private QuestionDto MapToDto(QuestionTableEntity entity)
        {
            return new QuestionDto
            {
                Id = entity.RowKey,
                Title = entity.Title,
                Description = entity.Description,
                PictureUrl = entity.PictureUrl,
                CreatedBy = entity.CreatedBy,
                TopAnswerId = entity.TopAnswerId,
                IsClosed = entity.IsClosed,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
        public async Task<QuestionDto> GetQuestionByIdAsync(string id)
        {
            var entity = await _questionRepository.GetQuestionByIdAsync(id);
            if (entity == null) return null;

            return new QuestionDto
            {
                Id = entity.RowKey,
                Title = entity.Title,
                Description = entity.Description,
                PictureUrl = entity.PictureUrl,
                CreatedBy = entity.CreatedBy,
                TopAnswerId = entity.TopAnswerId,
                IsClosed = entity.IsClosed,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
        public async Task<ValidationResponseDto> UpdateQuestionAsync(string id, UpdateQuestionDto dto, string email)
        {
            var questionEntity = await _questionRepository.GetQuestionByIdAsync(id);
            if (questionEntity == null)
            {
                return new ValidationResponseDto { Success = false, Errors = new List<FieldError> { new FieldError { Field = "id", Message = "Question not found." } } };
            }

            if (questionEntity.CreatedBy != email)
            {
                return new ValidationResponseDto { Success = false, Errors = new List<FieldError> { new FieldError { Field = "email", Message = "not Authorize." } } };
            }

            var errors = new List<FieldError>();
            if (string.IsNullOrWhiteSpace(dto.Title))
                errors.Add(new FieldError { Field = "title", Message = "Title is required." });
            if (string.IsNullOrWhiteSpace(dto.Description))
                errors.Add(new FieldError { Field = "description", Message = "Description is required." });

            if (errors.Any())
            {
                return new ValidationResponseDto { Success = false, Errors = errors };
            }

            if (dto.QuestionImage != null)
            {
                if (!string.IsNullOrEmpty(questionEntity.PictureUrl))
                {
                    await _pictureBlobService.DeleteFileAsync(questionEntity.PictureUrl);
                }
                var newPictureUrl = await _pictureBlobService.UploadFileAsync(dto.QuestionImage);
                questionEntity.PictureUrl = newPictureUrl;
            }

            questionEntity.Title = dto.Title;
            questionEntity.Description = dto.Description;
            questionEntity.UpdatedAt = DateTime.UtcNow;

            await _questionRepository.UpdateQuestionAsync(questionEntity);

            return new ValidationResponseDto { Success = true, Errors = new List<FieldError>() };
        }

        public async Task<bool> DeleteQuestionAsync(string id, string email)
        {
            var questionEntity = await _questionRepository.GetQuestionByIdAsync(id);
            if (questionEntity == null)
                return false;

            if (questionEntity.CreatedBy != email)
                return false;

            if (!string.IsNullOrEmpty(questionEntity.PictureUrl))
            {
                await _pictureBlobService.DeleteFileAsync(questionEntity.PictureUrl);
            }

            await _questionRepository.DeleteAnswersAndVotesForQuestionAsync(id);

            await _questionRepository.DeleteQuestionAsync(questionEntity);

            return true;
        }
        public async Task<List<QuestionDto>> GetQuestionsByUserEmailAsync(string email)
        {
            var entities = await _questionRepository.GetQuestionsByEmailAsync(email);
            return entities.Select(MapToDto).ToList();
        }
        public async Task<List<QuestionDto>> SearchQuestionsAsync(string title, DateTime? from, DateTime? to)
        {
            var allQuestions = await _questionRepository.GetAllQuestionsAsync();

            var filtered = allQuestions.Where(q =>
                (string.IsNullOrWhiteSpace(title) || q.Title.ToLower().Contains(title.ToLower())) &&
                (!from.HasValue || q.CreatedAt >= from.Value) &&
                (!to.HasValue || q.CreatedAt <= to.Value)
            );

            return filtered.Select(MapToDto).ToList();
        }

        public async Task<bool> CloseQuestionAsync(string questionId, string topAnswerId, string userEmail)
        {
            var question = await _questionRepository.GetQuestionByIdAsync(questionId);
            if (question == null || question.CreatedBy != userEmail || question.IsClosed)
                return false;

            await _questionRepository.CloseQuestionAsync(questionId, topAnswerId);
            return true;
        }
    }
}