using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Output
{
    [XmlType("User")]
    public class UsersOutputDto
    {
        [XmlElement("firstName")]
        public string FirstName { get; set; }
        
        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlArray("soldProducts")]
        public List<ProductsOutputDto> SoldProducts { get; set; }
    }
}
