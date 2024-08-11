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
    public class PersonHttpTrigger
    {
        private readonly ILogger _logger;

        public PersonHttpTrigger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpTrigger>();
        }

        [Function("GetPeople")]
        public async Task<HttpResponseData> GetFruit([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        { 
            using var streamReader = new StreamReader(req.Body);
            string requestBody = await streamReader.ReadToEndAsync();

            PostData MyPost = new(); 

            if (requestBody != null)
            {
                MyPost = JsonSerializer.Deserialize<PostData>(requestBody) ?? new(); 
            }   

            var faker = new Faker<BlazorApp.Shared.Person>().UseSeed(1519) 
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.BirthDate, f => f.Date.Past(50, DateTime.Now.AddYears(-18)));

            List<BlazorApp.Shared.Person> data = faker.Generate(50000);

            try 
            {
                string assemblyQualifiedName = $"BlazorApp.Shared.Person, Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
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
