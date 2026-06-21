using System.Collections.Generic;

namespace SDSMillimar.Dtos
{
    public class ProductDetailDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public List<TechnologyDto> Technologies { get; set; }
    }
}
