using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace P03_FootballBetting.Data.Models
{
    public class Position
    {
        public Position()
        {
            this.Players= new HashSet<Player>();
        }

        [Key]
        public int PositionId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(95)")]
        public string Name { get; set; }
        public virtual ICollection<Player> Players { get; set; }
    }
}
