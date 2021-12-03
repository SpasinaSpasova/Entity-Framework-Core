using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class DepartmentImportDto
    {
        [Required]
        [MaxLength(25)]
        [MinLength(3)]
        public string Name { get; set; }
        public CellImportDto[] Cells { get; set; }
    }
}
