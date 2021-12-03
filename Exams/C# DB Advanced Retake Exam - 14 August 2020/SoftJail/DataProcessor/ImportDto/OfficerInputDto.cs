using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficerInputDto
    {
        [XmlElement("Name")]
        [Required]
        [MaxLength(30)]
        [MinLength(3)]
        public string Name { get; set; }

        [XmlElement("Money")]
        [Range(0.0, (double)decimal.MaxValue)]
        public decimal Money { get; set; }

        [XmlElement("Position")]
        [Required]
        public string Position { get; set; }

        [XmlElement("Weapon")]
        [Required]
        public string Weapon { get; set; }

        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray]
        public PrisonerOfficerInputDto[] Prisoners { get; set; }
    }
}
