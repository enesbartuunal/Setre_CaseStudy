
using System.Collections.Generic;

namespace Setre.Models.Models.Filter
{
    public class FilterResponseModel<T> where T: class
    {
        public List<T> DataList { get; set; }

        public FilterPaggingInfo PaggingInfo { get; set; }
    }
}
