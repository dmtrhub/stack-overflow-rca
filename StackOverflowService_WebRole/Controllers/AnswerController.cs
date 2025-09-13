using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using StackOverflowService_WebRole.Repositories;
using StackOverflowService_WebRole.Services;
using StackOverflowService_WebRole.DTOs;


namespace StackOverflowService_WebRole.Controllers
{
    public class AnswerController : ApiController
    {
        private readonly AnswerService _answerService;

        public AnswerController()
        {
            var answerRepo = new AnswerRepository();
            _answerService = new AnswerService(answerRepo);
        }

        [Authorize]
        [HttpPost]
        [Route("api/answers/create")]
        public async Task<IHttpActionResult> Create([FromBody] CreateAnswerDto dto)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var email = identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var ret = await _answerService.CreateAsync(dto, email);
            if (ret.Success)
            {
                return Ok("Answer created successfuly");
            }
            return BadRequest();
        }
        [Authorize]
        [HttpGet]
        [Route("api/answers/by-question/{questionId}")]
        public async Task<IHttpActionResult> GetByQuestionId(string questionId)
        {
            var answers = await _answerService.GetAnswersByQuestionIdAsync(questionId);
            return Ok(answers);
        }
    }
}