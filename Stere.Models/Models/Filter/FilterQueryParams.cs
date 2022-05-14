

namespace Setre.Models.Models.Filter
{
    public class FilterQueryParams
    {
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
        public string[] SortOptions { get; set; } = null; 
        public bool SortingDirection { get; set; } = false; //false = asc, true = desc
        public string SearchValue { get; set; } = null;
    }

    
}
