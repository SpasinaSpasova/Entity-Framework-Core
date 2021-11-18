using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO.Input
{
    public class PartsInputDto
    {
        //"name": "Bonnet/hood",
        //"price": 1001.34,
        //"quantity": 10,
        //"supplierId": 17

        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SupplierId { get; set; }
    }
}
