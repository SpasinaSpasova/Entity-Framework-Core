using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace Theatre.DataProcessor.ImportDto
{
    [XmlType("Play")]
    public class PlayImportDto
    {
        [XmlElement("Title")]
        [Required]
        [MaxLength(50)]
        [MinLength(4)]
        public string Title { get; set; }

        [XmlElement("Duration")]
        [Required]
        //????in format {hours:minutes:seconds}, with a minimum length of 1 hour. 
        public string Duration { get; set; }

        [XmlElement("Rating")]
        [Range(0.00, 10.00)]
        public float Rating { get; set; }

        [XmlElement("Genre")]
        [Required]
        public string Genre { get; set; }

        [XmlElement("Description")]
        [Required]
        [MaxLength(700)]
        public string Description { get; set; }

        [XmlElement("Screenwriter")]
        [Required]
        [MaxLength(30)]
        [MinLength(4)]
        public string Screenwriter { get; set; }
    }
}
