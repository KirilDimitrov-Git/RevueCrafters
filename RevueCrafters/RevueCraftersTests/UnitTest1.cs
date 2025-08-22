using NUnit.Framework;
using NUnit.Framework.Interfaces;
using RestSharp;
using RestSharp.Authenticators;
using RevueCraftersTests.Models;
using System.Net;
using System.Text.Json;


namespace RevueCraftersTests
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedRevueId;
        public const string baseUrl = "https://d2925tksfvgq8c.cloudfront.net"; // Основния URL към сайта

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("Kiril@example.com", "parola123"); // Креденшълите към акаунта

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post); // Api-то за аутентикация

            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Test, Order(1)]
        public void CreateRevue_ShouldReturnCreated()
        {
            // Arrange
            var revue = new RevueDTO
            {
                Title = "Title",
                Url = "",
                Description = "Description"
            };

            // Act
            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(revue);
            var response = client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
            
        }
        [Test, Order(2)]
        public void GetAllRevues_ShouldReturnList()
        {
            // Act
            var request = new RestRequest("/api/Revue/All", Method.Get);
            var response = client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null);

            lastCreatedRevueId = responseItems.LastOrDefault()?.RevueId;

        }
        [Test, Order(3)]
        public void EditLastRevue_ShouldReturnEdited()
        {
            //Arrange
            
            var editRequest = new RevueDTO
            {
                Title = "New Titel",
                Url = "",
                Description = "New Description"
            };

            //Act

            var request = new RestRequest($"/api/Revue/Edit",Method.Put);
            request.AddQueryParameter("revueid", lastCreatedRevueId);
            request.AddJsonBody(editRequest);
            var response = client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }
        [Test, Order(4)]
        public void DeleteLastRevue_ShouldReturnDeleted()
        {

            //Act
            var request = new RestRequest($"/api/Revue/Delete", Method.Delete);
            request.AddQueryParameter("RevueId", lastCreatedRevueId);
            var response = client.Execute(request);
            var deletedResponse =JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deletedResponse.Msg, Is.EqualTo("The revue is deleted!"));
        }
        [Test, Order(5)]
        public void EditNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "123";
            var editRequest = new RevueDTO
            {
                Title = "New Title",
                Url = "",
                Description = "New Description"
            };

            var request = new RestRequest($"/api/Revue/Edit", Method.Put);
            request.AddQueryParameter("RevueId", nonExistingIdeaId);
            request.AddJsonBody(editRequest);
            var response = client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(editResponse.Msg, Is.EqualTo("There is no such revue!"));
        }
        [Test, Order(6)]
        public void DeleteNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "123";
            var request = new RestRequest($"/api/Revue/Delete", Method.Delete);
            request.AddQueryParameter("RevueId", nonExistingIdeaId);
            var response = client.Execute(request);
            var deletedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deletedResponse.Msg, Is.EqualTo("There is no such revue!"));
        }




        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }

    }
}
