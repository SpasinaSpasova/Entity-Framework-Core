using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Import
{
    public class UserInputDto
    {
        [Required]
        [RegularExpression(@"^[A-Z][a-z]{1,} [A-Z][a-z]{1,}$")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Range(3,103)]
        public int Age { get; set; }
        public CardInputDto[] Cards { get; set; }
    }
}
