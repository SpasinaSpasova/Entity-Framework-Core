using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class PrisonerImportDto
    {
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"The [A-Z][a-z]{1,}")]
        public string Nickname { get; set; }

        [Range(18,65)]
        public int Age { get; set; }

        [Required]
        public string IncarcerationDate { get; set; }
        public string ReleaseDate { get; set; }

        [Range(0.0, (double)decimal.MaxValue)]
        public decimal? Bail { get; set; }
        public int? CellId { get; set; }
        public MailImportDto[] Mails { get; set; }
    }
}
