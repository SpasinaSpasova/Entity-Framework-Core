using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.Dtos.Output
{
    public class UserSoldProductsOutputDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<SoldProdcutOutputDto> SoldProducts { get; set; }
    }
}
