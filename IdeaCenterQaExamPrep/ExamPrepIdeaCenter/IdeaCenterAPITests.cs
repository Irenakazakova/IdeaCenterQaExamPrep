using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrepIdeaCenter.Models;


namespace ExamPrepIdeaCenter
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedIdeaId; // Store the ID of the last created idea for use in subsequent tests

        private const string BaseUrl = "http://144.91.123.158:82";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJjZGJiZDYyNi01YjY1LTQ3NGYtYWUwMS0zZjAyMjg5NTU1OTYiLCJpYXQiOiIwNC8xNS8yMDI2IDE0OjIzOjM3IiwiVXNlcklkIjoiZmU1NTYwNTMtYmFmOC00MTUwLTUzOWItMDhkZTc2YTJkM2VjIiwiRW1haWwiOiIyMDI2cWFpa0BleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiMjAyNnFhaWsiLCJleHAiOjE3NzYyODQ2MTcsImlzcyI6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.cclxh4K6Ql_X09XIeisAYRvn82GasfSuG-mtnGgG3PY";
        private const string LoginEmail = "2026qaik@example.com";
        private const string LoginPassword = "2026qaik";

        [OneTimeSetUp]

        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/user/authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
               var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
               var token = content.GetProperty("token").GetString();

                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
                else
                {
                    throw new Exception("Token not found in response.");
                }
            }
            else
            {
                throw new Exception($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateIdea_with_requiredFields_ShouldReturnSuccess()
        {
            var ideaRequest = new ideaDTO
            {
                Title = "Test Idea Title",
                Description = "Test Idea Description",
                Url = ""
            };

            var request = new RestRequest("/api/idea/create", Method.Post);
            request.AddJsonBody(ideaRequest);

            var response = this.client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/idea/all", Method.Get);
            var response = this.client.Execute(request);
            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null.And.Not.Empty, "Expected at least one idea in the response.");
                        
            lastCreatedIdeaId = responseItems.LastOrDefault()?.Id; // Store the ID of the last idea for use in subsequent tests
            
        }

        [Order(3)]
        [Test]
        public void EditLastIdeaCreated_ShouldReturnSuccess()
        {
            if (string.IsNullOrEmpty(lastCreatedIdeaId))
            {
                Assert.Fail("No idea ID available from previous test. Ensure that the GetAllIdeas test runs before this one and that it retrieves at least one idea.");
            }
            var ideaEditRequest = new ideaDTO
            {
                Title = "Updated Test Idea Title",
                Description = "Updated Test Idea Description",
                Url = ""
            };
            var request = new RestRequest($"/api/idea/edit", Method.Put);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(ideaEditRequest);

            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Order(4)]
        [Test]
        public void DeleteLastIdeaCreated_ShouldReturnSuccess()
        {
            if (string.IsNullOrEmpty(lastCreatedIdeaId))
            {
                Assert.Fail("No idea ID available from previous test. Ensure that the GetAllIdeas test runs before this one and that it retrieves at least one idea.");
            }
            var request = new RestRequest($"/api/idea/delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]
        public void CreateIdeaWithoutRequiredFields_ShouldReturnBadRequest()
        {
            var ideaRequest = new ideaDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };
            var request = new RestRequest("/api/idea/create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditIdeaWithInvalidId_ShouldReturnBadRequest()
        {
            var ideaEditRequest = new ideaDTO
            {
                Title = "Updated Test Idea Title",
                Description = "Updated Test Idea Description",
                Url = ""
            };
            var request = new RestRequest($"/api/idea/edit", Method.Put);
            request.AddQueryParameter("ideaId", "invalid-id");
            request.AddJsonBody(ideaEditRequest);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

        [Order(7)]
        [Test]
        public void DeleteIdeaWithInvalidId_ShouldReturnBadRequest()
        {
            var request = new RestRequest($"/api/idea/delete", Method.Delete);
            request.AddQueryParameter("ideaId", "invalid-id");
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

        [OneTimeTearDown]
        public void TearDown() 
        { 
        this.client?.Dispose();
        }

    }
}