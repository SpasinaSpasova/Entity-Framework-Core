using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace P03_SalesDatabase.Data.Models
{
    public class Sale
    {
        [Key]
        public int SaleId { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }


        [ForeignKey("Customer")]

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }


        [ForeignKey("Store")]

        public int StoreId { get; set; }
        public Store Store { get; set; }
    }
}
