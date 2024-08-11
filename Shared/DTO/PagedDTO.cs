
namespace BlazorApp.Shared.DTO
{
    public class PagedDTO<T>
    {
        public T[] PagedItems {get;set;} = []; 
        public int TotalItems {get;set;} = 0;
    }
}
