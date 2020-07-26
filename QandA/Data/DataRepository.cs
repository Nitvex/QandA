using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Dapper;
using QandA.Data.Models;
using static Dapper.SqlMapper;

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
            using GridReader results = await connection.QueryMultipleAsync(
                @"EXEC dbo.Question_GetSingle @QuestionId = @QuestionId;
                EXEC dbo.Answer_Get_ByQuestionId @QuestionId = @QuestionId",
                new { QuestionId = questionId });

            QuestionGetSingleResponse question = (await results.ReadAsync<QuestionGetSingleResponse>()).FirstOrDefault();
            if (question != null)
            {
                question.Answers = (await results.ReadAsync<AnswerGetResponse>()).ToList();
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

        public async Task<QuestionGetSingleResponse> PostQuestionAsync(QuestionPostFullRequest question)
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

        public async Task<AnswerGetResponse> PostAnswerAsync(AnswerPostFullRequest answer)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstAsync<AnswerGetResponse>(
                @"EXEC dbo.Answer_Post
                @QuestionId = @QuestionId, @Content = @Content, @UserId = @UserId, @UserName = @UserName, @Created = @Created",
                answer
            );
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsWithAnswersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var questionDictionary = new Dictionary<int, QuestionGetManyResponse>();
            var result = await connection.QueryAsync<QuestionGetManyResponse, AnswerGetResponse, QuestionGetManyResponse>(
                "EXEC dbo.Question_GetMany_WithAnswers",
                map: (q, a) =>
                {
                    if (!questionDictionary.TryGetValue(q.QuestionId, out QuestionGetManyResponse question))
                    {
                        question = q;
                        question.Answers = new List<AnswerGetResponse>();
                        questionDictionary.Add(question.QuestionId, question);
                    }
                    question.Answers.Add(a);
                    return question;
                },
                splitOn: "QuestionId"
            );

            return result.Distinct().ToList();
        }

        public async Task<IEnumerable<QuestionGetManyResponse>> GetQuestionsBySearchWithPagingAsync(string search, int pageNumber, int pageSize)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<QuestionGetManyResponse>(
                @"EXEC dbo.Question_GetMany_BySearch_WithPaging @Search = @Search, @PageNumber=@PageNumber, @PageSize=@PageSize",
                new { Search = search, PageNumber = pageNumber, PageSize = pageSize }
            );
        }
    }
}
