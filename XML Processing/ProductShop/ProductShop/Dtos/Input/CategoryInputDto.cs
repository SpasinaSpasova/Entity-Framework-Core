using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Input
{
    [XmlType("Category")]
    public class CategoryInputDto
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }
}
