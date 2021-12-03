using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Cinema.DataProcessor.ImportDto
{
    [XmlType("Ticket")]
    public class TicketImportDto
    {
        [XmlElement("ProjectionId")]
        public int ProjectionId { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        [XmlElement("Price")]
        public decimal Price { get; set; }
    }
}