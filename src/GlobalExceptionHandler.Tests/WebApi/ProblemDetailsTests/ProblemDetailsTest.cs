using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GlobalExceptionHandler.ProblemDetails.Mvc;
using GlobalExceptionHandler.Tests.Exceptions;
using GlobalExceptionHandler.Tests.Fixtures;
using GlobalExceptionHandler.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace GlobalExceptionHandler.Tests.WebApi.ProblemDetailsTests
{
    public class ProblemDetailsTest : IClassFixture<WebApiServerFixture>
    {
        private readonly HttpClient _client;
        private readonly string _instance = "urn:dummyorganisation:notfound:" + Guid.NewGuid();

        private const string ApiProductNotFound = "/api/productnotfound";
        private const string ExceptionMessage = "Record could not be found";
        private const string ProblemDetailTitle = "There was an error with your request";

        public ProblemDetailsTest(WebApiServerFixture fixture)
        {
            // Arrange
            var webHost = fixture.CreateWebHostWithMvc();
            webHost.Configure(app =>
            {   
                app.UseGlobalExceptionHandler(x =>
                {
                    x.Map<RecordNotFoundException>().ToStatusCode(StatusCodes.Status404NotFound)
                        .WithProblemDetails(ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
                        {
                            Type = ex.GetType().Name,
                            Status = StatusCodes.Status404NotFound,
                            Detail = ex.Message,
                            Title = ProblemDetailTitle,
                            Instance = _instance
                        });
                });

                app.Map(ApiProductNotFound, config =>
                {
                    config.Run(context => throw new RecordNotFoundException(ExceptionMessage));
                });
            });

            _client = new TestServer(webHost).CreateClient();
        }
        
        [Fact]
        public async Task Returns_correct_response_type()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
        }

        [Fact]
        public async Task Returns_correct_status_code()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Returns_correct_problem_detail()
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("GET"), ApiProductNotFound));
            var content = await response.Content.ReadAsStringAsync();
            var detail = JsonConvert.DeserializeObject<Microsoft.AspNetCore.Mvc.ProblemDetails>(content);
            
            detail.Detail.ShouldBe(ExceptionMessage);
            detail.Type.ShouldBe(typeof(RecordNotFoundException).Name);
            detail.Title.ShouldBe(ProblemDetailTitle);
            detail.Status.ShouldBe(StatusCodes.Status404NotFound);
            detail.Instance.ShouldBe(_instance);
        }
    }
}