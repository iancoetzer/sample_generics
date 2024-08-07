using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization ;
//using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using BlazorApp.Shared.Converters;

namespace BlazorApp.Shared
{
    public class Person: ITableEntity
    {
        // Azure Table Storage - Default Properties
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";

        public DateTimeOffset? Timestamp { get; set; }

        //[JsonConverter(typeof(ETagCustomConverter))]
        [JsonConverter(typeof(ETagConverter))]
        public ETag ETag {get;set;}

        // Custom Properties
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        private DateTime? _birthDate {get;set;}
        public DateTime? BirthDate{
            get => _birthDate;
            set => _birthDate = value.HasValue? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?)null;
        }


        // Default Constructor
        public Person()
        {
            if (_birthDate != null)
                _birthDate = DateTime.SpecifyKind(_birthDate.Value, DateTimeKind.Utc);
        }             
    }
}
