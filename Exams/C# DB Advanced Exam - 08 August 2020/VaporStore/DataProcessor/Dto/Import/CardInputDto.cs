using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class CardInputDto
    {
        [Required]
        [MaxLength(19)]
        [RegularExpression(@"^\d{4} \d{4} \d{4} \d{4}$")]
        public string Number { get; set; }

        [Required]
        [MaxLength(3)]
        [RegularExpression(@"^\d{3}$")]
        public string CVC { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
