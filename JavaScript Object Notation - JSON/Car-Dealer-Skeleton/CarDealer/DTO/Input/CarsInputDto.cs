using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO.Input
{
    public class CarsInputDto
    {
        //"make": "Opel",
        //"model": "Omega",
        //"travelledDistance": 176664996,
        //"partsId"

        public string Make { get; set; }
        public string Model { get; set; }
        public long TravelledDistance { get; set; }
        public int[] PartsId { get; set; }

    }
}
