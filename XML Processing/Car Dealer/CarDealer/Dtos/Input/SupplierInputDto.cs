using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Input
{
    [XmlType("Supplier")]
    public class SupplierInputDto
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("IsImporter")]
        public bool IsImporter { get; set; }
    }
}
