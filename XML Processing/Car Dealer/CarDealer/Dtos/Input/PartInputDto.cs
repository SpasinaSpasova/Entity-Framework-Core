using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Input
{
    [XmlType("Part")]
    public class PartInputDto
    {
        // <name>Bonnet/hood</name>
        //<price>1001.34</price>
        //<quantity>10</quantity>
        //<supplierId>17</supplierId>

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }
       
        [XmlElement("quantity")]
        public int Quantity { get; set; }

        [XmlElement("supplierId")]
        public int supplierId { get; set; }
    }
}
