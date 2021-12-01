using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace BookShop.DataProcessor.ImportDto
{
    [XmlType("Book")]
    public class BookImportDto
    {
        [XmlElement("Name")]
        [MaxLength(30)]
        [MinLength(3)]
        [Required]
        public string Name { get; set; }

        [XmlElement("Genre")]
        [Range(1, 3)]
        public int Genre { get; set; }

        [XmlElement("Price")]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        [Range(50, 5000)]
        [XmlElement("Pages")]
        public int Pages { get; set; }

        [XmlElement("PublishedOn")]
    
        [Required]
        public string PublishedOn { get; set; }
    }
}
