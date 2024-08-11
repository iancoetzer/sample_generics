using System.Text; 
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Bogus;
using Azure;
using BlazorApp.Shared.Helpers;
using BlazorApp.Shared.DTO;

namespace Api
{
    public class FruitHttpTrigger
    {
        private readonly ILogger _logger;

        public FruitHttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTrigger>();
        }

        [Function("GetFruit")]
        public async Task<HttpResponseData> GetFruit([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {            
            using var streamReader = new StreamReader(req.Body);
            string requestBody = await streamReader.ReadToEndAsync();

            PostData MyPost = new(); 

            if (requestBody != null)
            {
                MyPost = JsonSerializer.Deserialize<PostData>(requestBody) ?? new(); 
            }            

            var fruitFaker = new Faker<Fruit>().UseSeed(1500103)
                .RuleFor(f => f.Name, f => f.PickRandom(new[] { "Apple", "Banana", "Orange", "Strawberry", "Kiwi" }))
                .RuleFor(f => f.Color, f => f.Commerce.Color())
                .RuleFor(f => f.Weight, f => f.Random.Double(50, 300)) // Weight in grams
                .RuleFor(f => f.Taste, f => f.PickRandom(new[] { "Sweet", "Sour", "Bitter", "Salty" }))
                .RuleFor(f => f.Season, f => f.PickRandom(new[] { "Spring", "Summer", "Autumn", "Winter" }))
                .RuleFor(f => f.NutritionalValue, f => f.Commerce.ProductAdjective())
                .RuleFor(f => f.IsRipe, f => f.Random.Bool())
                .RuleFor(f => f.StockDate, f => f.Date.Between(new DateTime(2022, 1, 1), DateTime.Now))
                .RuleFor(f => f.Price, f => f.Random.Decimal(1, 10)) // Price in dollars
                .RuleFor(f => f.ETag, f => new ETag($"\"{f.Random.Number(1000, 9999)}\"")); // ETag as a random number

            List<Fruit> data = fruitFaker.Generate(50000);


            try 
            {
                string assemblyQualifiedName = $"BlazorApp.Shared.Fruit, Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                Type? type = Type.GetType (assemblyQualifiedName); 
                                
                if (type != null)
                {
                    var genericMethod = typeof(GenericFilterSortAndPageHelper).GetMethod( "GenericFilterSortAndPage");
                    if (genericMethod != null)
                    {
                        var genericMethodAction = genericMethod.MakeGenericMethod(type); 
                        if (genericMethodAction != null) 
                        {
                            Type tupleType = typeof(Tuple<,>).MakeGenericType(type.MakeArrayType(), typeof(int));
                            
                            var pagedData = genericMethodAction.Invoke(this, [
                                data.AsEnumerable(), 
                                MyPost.searchString, 
                                MyPost.sortLabel, 
                                MyPost.sortDirection, 
                                MyPost.page, 
                                MyPost.pageSize, 
                                MyPost.searchFields, 
                                MyPost.sortFields 
                            ])!; // Null-forgiving operator 

                            var mytuple = (dynamic)Convert.ChangeType(pagedData, tupleType);

                            Type pagedDtoType = typeof(PagedDTO<>).MakeGenericType(type);
                            var dto = Activator.CreateInstance(pagedDtoType)!; // Null-forgiving operator
                            pagedDtoType.GetProperty("PagedItems")?.SetValue(dto, mytuple.Item1);
                            pagedDtoType.GetProperty("TotalItems")?.SetValue(dto, mytuple.Item2);                            

                            var response = req.CreateResponse(HttpStatusCode.OK);
                            await response.WriteAsJsonAsync(dto);

                            return response;                        
                        }
                    }
                }

                var responseError = req.CreateResponse(HttpStatusCode.InternalServerError);
                await responseError.WriteAsJsonAsync($"The server encountered an exception!");

                return responseError;
            }
            catch(Exception ex)
            {
                var responseError = req.CreateResponse(HttpStatusCode.InternalServerError);
                await responseError.WriteAsJsonAsync($"The server encountered an exception! {ex.Message}");

                return responseError;
            }
        }

        private class PostData 
        {
            public string searchString { get; set; } = string.Empty;
            public string sortLabel { get; set; } = string.Empty;
            public int sortDirection { get; set; }
            public int page { get; set; }
            public int pageSize { get; set; }
            public List<string> searchFields { get; set; } = [];
            public Dictionary<string, string> sortFields { get; set; } = [];

            public Fruit[] items {get;set;} = [];
        }        
    }
}
