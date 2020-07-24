using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QandA.Data;
using QandA.Data.Models;

namespace QandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;

        public QuestionsController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsAsync(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return await _dataRepository.GetQuestionsAsync();
            }
            else
            {
                return await _dataRepository.GetQuestionsBySearchAsync(search);
            }
        }

        [HttpGet("unanswered")]
        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync()
        {
            return await _dataRepository.GetUnansweredQuestionsAsync();
        }

        [HttpGet("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> GetQuestionAsync(int questionId)
        {
            var question = await _dataRepository.GetQuestionAsync(questionId);
            if (question == null)
            {
                return NotFound();
            }
            return question;
        }

        [HttpPost]
        public async Task<ActionResult<QuestionGetSingleResponse>> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            var savedQuestion = await _dataRepository.PostQuestionAsync(questionPostRequest);
            return CreatedAtAction(nameof(GetQuestionAsync),
                   new { questionId = savedQuestion.QuestionId },
                   savedQuestion
            );
        }

        [HttpPut("{questionId}")]
        public async Task<ActionResult<QuestionGetSingleResponse>> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {
            var question = await _dataRepository.GetQuestionAsync(questionId);
            if (question == null)
            {
                return NotFound();
            }

            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title)
                ? question.Title
                : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content)
                ? question.Content
                : questionPutRequest.Content;

            var savedQuestion = await _dataRepository.PutQuestionAsync(questionId, questionPutRequest);
            return savedQuestion;
        }

        [HttpDelete("{questionId}")]
        public async Task<ActionResult> DeleteQuestionAsync(int questionId)
        {
            var question = await _dataRepository.GetQuestionAsync(questionId);
            if (question == null)
            {
                return NotFound();
            }
            await _dataRepository.DeleteQuestionAsync(questionId);
            return NoContent();
        }
    }
}
