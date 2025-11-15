using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureRetailHub.Functions.Data
{
    public class OrderItem
    {
        [Key]
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