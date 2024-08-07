using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization ;
using Azure.Data.Tables;
using Azure;
using BlazorApp.Shared.Converters;

namespace BlazorApp.Shared
{
    public class Fruit: ITableEntity
    {
        // Azure Table Storage - Default Properties
        public string PartitionKey { get; set; } = "";
        public string RowKey { get; set; } = "";

        public DateTimeOffset? Timestamp { get; set; }

        [JsonConverter(typeof(ETagConverter))]
        public ETag ETag { get; set; }        

        // Custom Properties
        private DateTime? _stockDate {get;set;}
        public DateTime? StockDate{
            get => _stockDate;
            set => _stockDate = value.HasValue? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?)null;
        }

        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string Taste { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string NutritionalValue { get; set; } = string.Empty;
        public bool IsRipe { get; set; }
        public decimal Price { get; set; }
        
        // Default Constructor
        public Fruit()
        {
            if (_stockDate != null)
                _stockDate = DateTime.SpecifyKind(_stockDate.Value, DateTimeKind.Utc);
        }             
    }
}
