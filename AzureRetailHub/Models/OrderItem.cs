/*
 * ST10447759
 * CLDV6212
 * Part 3
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureRetailHub.Models
{
    // This table links Orders to Products and stores quantity/price
    public class OrderItem
    {
        public int Id { get; set; } // Simple int primary key

        [Required]
        public string OrderId { get; set; } = "";
        public virtual Order? Order { get; set; }

        [Required]
        public string ProductId { get; set; } = "";
        public virtual Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Price at the time of purchase
    }
}