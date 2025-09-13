using StackOverflowService_WebRole.DTOs;
using StackOverflowService_WebRole.Models;
using StackOverflowService_WebRole.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Http;
using System.Web.Http.Cors;
using StackOverflowService_WebRole.Services;

namespace StackOverflowService_WebRole.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class VoteController : ApiController
    {

        private readonly VoteService _voteService;

        public VoteController()
        {
            var voteRepo = new VoteRepository();
            var answerRepo = new AnswerRepository();
            _voteService = new VoteService(voteRepo, answerRepo);
        }

        [Authorize]
        [HttpPost]
        [Route("api/vote/create/{id}")]
        public async Task<IHttpActionResult> Create(string id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var email = identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var dto = new CreateVoteDto()
            {
                AnswerId = id,
                VotedByEmail = email
            };

            var ret = await _voteService.CreateVoteAsync(dto);

            if (ret)
            {
                return Ok("Vote created successfully.");
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, "Question not created.");
            }
        }
        [Authorize]
        [HttpDelete]
        [Route("api/vote/delete/{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var email = identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var success = await _voteService.DeleteVoteAsync(id, email);

            if (success)
                return Ok("Vote removed successfully.");
            else
                return Content(HttpStatusCode.BadRequest, "Vote could not be removed.");
        }

        [Authorize]
        [HttpGet]
        [Route("api/vote/has-voted/{id}")]
        public async Task<IHttpActionResult> HasVoted(string id)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var email = identity.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var hasVoted = await _voteService.HasUserVotedAsync(id, email);
            return Ok(hasVoted);
        }

    }
}