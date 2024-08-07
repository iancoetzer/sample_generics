using System;
using System.Dynamic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Newtonsoft.Json;
using BlazorApp.Shared.Converters;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; 

namespace BlazorApp.Shared.Helpers
{
    public class GenericFilterSortAndPageHelper
    {
        public static Tuple<T[], int> GenericFilterSortAndPage<T>(
            IEnumerable<T> data, 
            string searchString, 
            string sortLabel, 
            int sortDirection, 
            int page, 
            int pageSize,
            List<string> searchFields,
            Dictionary<string,string> sortFields)            
        where T: class 
        {
            // convert the listing to expando objects
            List<ExpandoObject> rows = data.Select(x=> ConvertToExpandoObject(x)).ToList();

            Tuple<ExpandoObject[], int> pagedTable = ExpandoFilterSortAndPage(
                rows, searchString, sortLabel, 
                sortDirection, page, pageSize,
                searchFields, sortFields
            );

            List<ExpandoObject> records = pagedTable.Item1?.ToList() ??
                new List<ExpandoObject>();

            Tuple<T[], int> result = new(
                records.Select(x=>
                    {
                        var dict = (IDictionary<string, object?>)x;

                        var options = new JsonSerializerOptions
                        {
                            Converters = { new ETagConverter() }
                        };

                        string json = JsonSerializer.Serialize(dict, options);

                        T? item = JsonSerializer.Deserialize<T>(json, options);
                        return item;
                    }
                    ).ToArray()! //ToList()!.Cast<T>()
                    ,
                    pagedTable.Item2
            );

            return result;
        }

        private static Tuple<ExpandoObject[], int> ExpandoFilterSortAndPage(
            IEnumerable<ExpandoObject> data, 
            string searchString, 
            string sortLabel, 
            int sortDirection, 
            int page, 
            int pageSize,
            List<string> searchFields,
            Dictionary<string,string> sortFields)
        {
            data = data.Where(element =>
            {
                if (string.IsNullOrWhiteSpace(searchString))
                    return true;

                var expandoDict = (IDictionary<string, object?>) element;

                bool result = false; 

                result = searchFields.Any(field =>
                {
                    if (!expandoDict.ContainsKey(field))
                        return false;

                    var value = expandoDict[field];

                    if (value is double doubleValue)
                        return doubleValue.ToString("N2").Contains(searchString, StringComparison.OrdinalIgnoreCase);
                    else if (value is decimal decimalValue)
                        return decimalValue.ToString("N2").Contains(searchString, StringComparison.OrdinalIgnoreCase);
                    else if (value is DateTime dateTimeValue)
                        return dateTimeValue.ToString("yyyy/MM/dd").Contains(searchString, StringComparison.OrdinalIgnoreCase);

                    return (value?.ToString() ?? "").Contains(searchString, StringComparison.OrdinalIgnoreCase);
                });

                return result;
            }).ToArray();            


            if (sortLabel != null)
            {
                // 0 = none
                // 1 = Ascending
                // 2 = Descending 

                if (sortDirection == 1)
                    data = data.OrderBy(x => ((IDictionary<string, object?>)x)[sortFields[sortLabel]]);

                else if (sortDirection ==2) 
                    data = data.OrderByDescending(x=> ((IDictionary<string,object?>)x)[sortFields[sortLabel]]);
            }

            IEnumerable<ExpandoObject> pagedData = data.Skip(page * pageSize).Take(pageSize).ToArray();

            Tuple<ExpandoObject[], int> responseObject = new(pagedData.ToArray(), data.Count());
            return responseObject;  
        }        

        private static ExpandoObject ConvertToExpandoObject<T>(T typedObject)
        {
            var expando = new ExpandoObject();
            var dictionary = (IDictionary<string, object?>)expando;

            foreach (var property in typeof(T).GetProperties())
            {
                dictionary[property.Name] = property.GetValue(typedObject);
            }

            return expando;
        }        
    }  
}
