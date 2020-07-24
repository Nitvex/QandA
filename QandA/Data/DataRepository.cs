using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using QandA.Data.Models;

namespace QandA.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public async Task<AnswerGetResponse> GetAnswerAsync(int answerId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<AnswerGetResponse>(
                @"EXEC dbo.Answer_Get_ByAnswerId @AnswerId = @AnswerId",
                new { AnswerId = answerId }
            );
        }

        public async Task<QuestionGetSingleResponse> GetQuestionAsync(int questionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var question = await connection.QueryFirstOrDefaultAsync<QuestionGetSingleResponse>(
                @"EXEC dbo.Question_GetSingle @QuestionId = @QuestionId",
                new { QuestionId = questionId }
            );

            if (question != null)
            {
                question.Answers = await connection.QueryAsync<AnswerGetResponse>(
                    @"EXEC dbo.Answer_Get_ByQuestionId @QuestionId = @QuestionId",
                    new { QuestionId = questionId }
                );
            }

            return question;
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<QuestionGetManyResponse>(@"EXEC dbo.Question_GetMany");
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchAsync(string search)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<QuestionGetManyResponse>(
                @"EXEC dbo.Question_GetMany_BySearch @Search = @Search", new { Search = search }
            );
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetUnansweredQuestionsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<QuestionGetManyResponse>(
                "EXEC dbo.Question_GetUnanswered"
            );
        }

        public async Task<bool> QuestionExistsAsync(int questionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstAsync<bool>(
                @"EXEC dbo.Question_Exists @QuestionId = @QuestionId",
                new { QuestionId = questionId }
            );
        }

        public async Task<QuestionGetSingleResponse> PostQuestionAsync(QuestionPostRequest question)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var questionId = await connection.QueryFirstAsync<int>(
                @"EXEC dbo.Question_Post
                @Title = @Title, @Content = @Content, @UserId = @UserId, @UserName = @UserName, @Created = @Created",
                question
            );
            return await GetQuestionAsync(questionId);
        }

        public async Task<QuestionGetSingleResponse> PutQuestionAsync(int questionId, QuestionPutRequest question)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                @"EXEC dbo.Question_Put
                @QuestionId = @QuestionId, @Title = @Title, @Content = @Content",
                new { QuestionId = questionId, question.Title, question.Content }
            );
            return await GetQuestionAsync(questionId);
        }

        public async Task DeleteQuestionAsync(int questionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await connection.ExecuteAsync(
                @"EXEC dbo.Question_Delete @QuestionId = @QuestionId",
                new { QuestionId = questionId }
            );
        }

        public async Task<AnswerGetResponse> PostAnswerAsync(AnswerPostRequest answer)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstAsync<AnswerGetResponse>(
                @"EXEC dbo.Answer_Post
                @QuestionId = @QuestionId, @Content = @Content, @UserId = @UserId, @UserName = @UserName, @Created = @Created",
                answer
            );
        }
    }
}
