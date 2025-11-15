using System.ComponentModel.DataAnnotations;

namespace AzureRetailHub.Functions.Data
{
    public class Order
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string? Status { get; set; } = "New";

        // Foreign Key to link to the User
        [Required]
        public string UserId { get; set; } = "";

        // Navigation property
        public virtual User? User { get; set; }

        // Navigation property for all items in this order
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}