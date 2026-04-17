using System.Net;
using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using FoodyTest1.Models;

namespace FoodyTest1
{
    

    [TestFixture]

    public class Tests
    {

        private RestClient client;
        private static string foodId; //id na създадената храна, която ще използваме в следващите тестове

        private const string BaseUrl = "http://144.91.123.158:81";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIxYWUxZjIyZi1hNjU5LTRkMzYtOTg3My1iNWU2ZGU3M2IwNDUiLCJpYXQiOiIwNC8xNy8yMDI2IDE0OjA4OjAyIiwiVXNlcklkIjoiYjMwZjczNTktNGNmMC00ZmYxLTc0OGItMDhkZTc2OGU4Zjk3IiwiRW1haWwiOiJ1c2VyODk4NUBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiemxhdDY5ODciLCJleHAiOjE3NzY0NTY0ODIsImlzcyI6IkZvb2R5X0FwcF9Tb2Z0VW5pIiwiYXVkIjoiRm9vZHlfV2ViQVBJX1NvZnRVbmkifQ.HQs0uNF4PYXJqs0ZD9fyQINPeGuIFl3a_svGy0ocjbk";
        private const string LoginUserName = "zlat6987";
        private const string LoginPassword = "string6363";

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
                jwtToken = GetJwtToken(LoginUserName, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }
        private string GetJwtToken(string userName, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });

            var response = tempClient.Execute(request);  //получаваме респонс от сървъра, след като сме му изпратили юзърнейм и пасуорд за аутентикация

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status Code: {response.StatusCode}, Response: {response.Content}");
            }

        }
        [Order(1)]
        [Test]
        public void CreateNewFood_WithRequiredFields_ShouldReturnSuccess()
        {
            var foodData = new FoodDTO
            {
                Name = "Test Food",
                Description = "This is a test food item.",
                Url = ""
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodData);

            RestResponse response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            foodId = createResponse.FoodId;


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), $"Expected status code 201 Created ");
            Assert.That(foodId, Is.Not.Null.And.Not.Empty, "Food ID schould not be null or empty");

        }

        [Order(2)]
        [Test]

        public void EditTitel_OfTheCreatedFood_SchouldReturnSuccess()
        {

            RestRequest request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);
            request.AddBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Chicken Soup"
                }
            });

            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]

        public void GetAllFoods_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var readyResponse = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
        }

        [Order(4)]
        [Test]

        public void DeleteCreatedFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void CreatedFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var foodData = new FoodDTO
            {
                Name = "",
                Description = "This is a test food item without a name.",
                Url = ""
            };
            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(foodData);
            RestResponse response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Expected status code 400 Bad Request");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingFood_ShouldReturnNotFound()
        {


            /* string nonExistingFoodId = "9999999";
             var editRequestData = new FoodDTO

             {
                 Name = "Edited Food",
                 Description = "This is a edited food description.",
                 Url = ""
             };
             var request = new RestRequest("/api/Food/Edit", Method.Put);
             request.AddQueryParameter("foodId", nonExistingFoodId);
             request.AddJsonBody(editRequestData);

             var response = this.client.Execute(request);

             ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
             Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));

             Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            */



            string nonExistingFoodId = "156781";

            RestRequest request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
            request.AddBody(new[]
                       {
                  new
                  {
                      path = "/name",
                      op = "replace",
                      value = "Chicken Soup"
                  }
              });

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));


        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingFood_ShouldReturnNotFound()
        {


            string nonExistingFoodId = "156781";

            RestRequest request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));

        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }

}