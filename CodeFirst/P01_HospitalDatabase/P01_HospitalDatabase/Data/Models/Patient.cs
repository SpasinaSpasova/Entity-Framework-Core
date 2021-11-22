using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace P01_HospitalDatabase.Data.Models
{
    public class Patient
    {
        public Patient()
        {
            this.Visitations = new HashSet<Visitation>();
            this.Diagnoses = new HashSet<Diagnose>();
            this.Prescriptions = new HashSet<PatientMedicament>();
        }
        [Key]
        public int PatientId { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string FirstName  { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string LastName { get; set; }

        [MaxLength(250)]
        [Column(TypeName = "nvarchar(250)")]
        public string Address { get; set; }

        [MaxLength(80)]
        [Column(TypeName = "varchar(80)")]
        public string Email  { get; set; }

        public bool HasInsurance { get; set; }

        public ICollection<Visitation> Visitations { get; set; }

        public ICollection<Diagnose> Diagnoses { get; set; }

        public ICollection<PatientMedicament> Prescriptions { get; set; }
    }
}
