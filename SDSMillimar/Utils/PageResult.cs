using System.Collections.Generic;

namespace SDSMillimar.Utils
{
    public class PageResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }

        public PageResult()
        {
            Items = new List<T>();
        }
    }
}
